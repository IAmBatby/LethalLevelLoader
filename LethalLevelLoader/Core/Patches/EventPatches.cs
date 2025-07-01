using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    //This class is dedicated to the patches needed to collect data sent to events inside the current ExtendedLevel and ExtendedDungeonFlow.
    //They are separated for organisation purposes and to enforce that all of these patches should only be reading information and sending it off
    //Nothing in this class should modify the game in any way.
    internal class EventPatches
    {
        internal static DayMode previousDayMode = DayMode.None;
        internal static bool firedDawnEvent = false;

        internal static bool IsServer => NetworkManager.Singleton.IsServer;
        internal static ExtendedLevel CurrentLevel => LevelManager.CurrentExtendedLevel;
        internal static ExtendedDungeonFlow CurrentDungeon => DungeonManager.CurrentExtendedDungeonFlow;
        internal static LevelEvents[] LevelEvents => new [] { LevelManager.GlobalLevelEvents, CurrentLevel?.LevelEvents };
        internal static DungeonEvents[] DungeonEvents => new[] {DungeonManager.GlobalDungeonEvents, CurrentDungeon?.DungeonEvents }; 

        internal static void InvokeExtendedEvent<T>(ExtendedEvent<T> extendedEvent, T eventParameter)
        {
            extendedEvent.Invoke(eventParameter);
        }

        internal static void Invoke(IEnumerable<ExtendedEvent> events)
        {
            foreach (ExtendedEvent extendedEvent in events)
                extendedEvent.Invoke();
        }

        internal static void InvokeIf(bool condition, IEnumerable<ExtendedEvent> events)
        {
            if (condition)
                Invoke(events);
        }

        internal static void Invoke<T>(IEnumerable<ExtendedEvent<T>> events, T value)
        {
            foreach (ExtendedEvent<T> extendedEvent in events)
                extendedEvent.Invoke(value);
        }

        internal static void InvokeIf<T>(bool condition, IEnumerable<ExtendedEvent<T>> events, T value)
        {
            if (condition)
                Invoke(events, value);
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (CurrentLevel == null || CurrentLevel.IsLevelLoaded == false) return;
            previousDayMode = DayMode.None;
            Invoke(LevelEvents.Select(e => e.onLevelLoaded));
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(StoryLog), "CollectLog"), HarmonyPrefix]
        internal static void StoryLogCollectLog_Prefix(StoryLog __instance)
        {
            InvokeIf(CurrentLevel && IsServer, LevelEvents.Select(e => e.onStoryLogCollected), __instance);
        }

        [HarmonyPriority(Patches.priority + 1), HarmonyPatch(typeof(DungeonGenerator), "Generate"), HarmonyPrefix] // +1 Because this needs to run after the Patch in Patches, second patch here for consistency.
        internal static void DungeonGeneratorGenerate_Prefix()
        {
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onBeforeDungeonGenerate), Patches.RoundManager);
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SwitchPower"), HarmonyPrefix]
        internal static void RoundManagerSwitchPower_Prefix(bool on)
        {
            InvokeIf(CurrentLevel != null, LevelEvents.Select(e => e.onPowerSwitchToggle), on);
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onPowerSwitchToggle), on);
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel"), HarmonyPostfix]
        internal static void RoundManagerSpawnScrapInLevel_Postfix()
        {
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onSpawnedScrapObjects), UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList());
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SpawnSyncedProps"), HarmonyPostfix]
        internal static void RoundManagerSpawnSyncedProps_Postfix()
        {
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onSpawnedSyncedObjects), Patches.RoundManager.spawnedSyncedObjects);
        }

        private static EnemyVent cachedSelectedVent;
        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent"), HarmonyPrefix]
        internal static void RoundManagerSpawnEventFromVent_Prefix(EnemyVent vent)
        {
            cachedSelectedVent = CurrentDungeon != null ? vent : cachedSelectedVent;
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SpawnEnemyGameObject"), HarmonyPostfix]
        internal static void RoundManagerSpawnEventFromVent_Postfix()
        {
            if (CurrentDungeon == null || cachedSelectedVent == null) return;
            Invoke(DungeonEvents.Select(e => e.onEnemySpawnedFromVent), (cachedSelectedVent, Patches.RoundManager.SpawnedEnemies.Last()));
            cachedSelectedVent = null;
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(RoundManager), "SpawnMapObjects"), HarmonyPostfix]
        internal static void RoundManagerSpawnMapObjects_Postfix()
        {
            if (CurrentDungeon == null) return;

            List<GameObject> mapObjects = new List<GameObject>();
            foreach (GameObject rootObject in SceneManager.GetSceneByName(CurrentLevel.SelectableLevel.sceneName).GetRootGameObjects())
                foreach (SpawnableMapObject randomMapObject in CurrentLevel.SelectableLevel.spawnableMapObjects)
                    if (rootObject.name.Sanitized().Contains(randomMapObject.prefabToSpawn.name.Sanitized())) //To ensure were only getting the Dungeon relevant objects.
                        mapObjects.Add(rootObject);
            Invoke(DungeonEvents.Select(e => e.onSpawnedMapObjects), mapObjects);
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(StartOfRound), "OnShipLandedMiscEvents"), HarmonyPrefix]
        internal static void StartOfRoundOnShipLandedMiscEvents_Prefix()
        {
            InvokeIf(CurrentLevel != null, LevelEvents.Select(e => e.onShipLand));
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onShipLand));
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(StartOfRound), "ShipLeave"), HarmonyPrefix]
        internal static void StartOfRoundShipLeave_Prefix()
        {
            InvokeIf(CurrentLevel != null, LevelEvents.Select(e => e.onShipLeave));
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onShipLeave));
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayerServerRpc"), HarmonyPrefix]
        internal static void EntranceTeleportTeleportPlayerServerRpc_Prefix(EntranceTeleport __instance, int playerObj)
        {
            if (!IsServer) return;
            InvokeIf(CurrentLevel != null, LevelEvents.Select(e => __instance.isEntranceToBuilding ? e.onPlayerEnterDungeon : e.onPlayerExitDungeon), (__instance, Patches.StartOfRound.allPlayerScripts[playerObj]));
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => __instance.isEntranceToBuilding ? e.onPlayerEnterDungeon : e.onPlayerExitDungeon), (__instance, Patches.StartOfRound.allPlayerScripts[playerObj]));
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(LungProp), "EquipItem"), HarmonyPrefix]
        internal static void LungPropEquipItem_Prefix(LungProp __instance)
        {
            if (IsServer == false || __instance.isLungDocked == false) return;
            InvokeIf(CurrentLevel != null, LevelEvents.Select(e => e.onApparatusTaken), __instance);
            InvokeIf(CurrentDungeon != null, DungeonEvents.Select(e => e.onApparatusTaken), __instance);
        }

        [HarmonyPriority(Patches.priority), HarmonyPatch(typeof(TimeOfDay), "GetDayPhase"), HarmonyPostfix]
        internal static void TimeOfDayGetDayPhase_Postfix(DayMode __result)
        {
            InvokeIf(CurrentLevel != null && (previousDayMode == DayMode.None || previousDayMode != __result), LevelEvents.Select(e => e.onDayModeToggle), __result);
            previousDayMode = __result;
        }
    }
}
