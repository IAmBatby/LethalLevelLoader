using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using IL;
using LethalLib.Modules;
using On;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    //This class is dedicated to the patches needed to collect data sent to events inside the current ExtendedLevel and ExtendedDungeonFlow.
    //They are seperated for organisation purposes and to enforce that all of these patches should only be reading information and sending it off
    //Nothing in this class should modify the game in any way.
    internal class EventPatches
    {
        public static LevelEvents GlobalLevelEvents = new LevelEvents();
        public static DungeonEvents DungeonEvents = new DungeonEvents();
        internal static DayMode previousDayMode = DayMode.None;
        internal static bool firedDawnEvent = false;
        ////////// Level Patches //////////
        
        internal static void InvokeExtendedEvent<T>(ExtendedEvent<T> extendedEvent, T eventParameter)
        {
            extendedEvent.Invoke(eventParameter);
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (LevelManager.CurrentExtendedLevel != null && LevelManager.CurrentExtendedLevel.IsLoadedLevel)
            {
                previousDayMode = DayMode.None;
                LevelManager.CurrentExtendedLevel.levelEvents.onLevelLoaded.Invoke();
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(StoryLog), "CollectLog")]
        [HarmonyPrefix]
        internal static void StoryLogCollectLog_Prefix(StoryLog __instance)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
                LevelManager.CurrentExtendedLevel.levelEvents.onStoryLogCollected.Invoke(__instance);
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnRandomDaytimeEnemy")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnRandomDaytimeEnemy_Postfix(RoundManager __instance, GameObject __result)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
                if (__result != null && __result.TryGetComponent(out EnemyAI enemyAI))
                    LevelManager.CurrentExtendedLevel.levelEvents.onDaytimeEnemySpawn.Invoke(enemyAI);
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnRandomOutsideEnemy_Postfix(RoundManager __instance, GameObject __result)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
                if (__result != null && __result.TryGetComponent(out EnemyAI enemyAI))
                    LevelManager.CurrentExtendedLevel.levelEvents.onNighttimeEnemySpawn.Invoke(enemyAI);
        }




        ////////// Dungeon Patches //////////

        [HarmonyPriority(Patches.harmonyPriority + 1)] // +1 Because this needs to run after the Patch in Patches, second patch here for consistency.
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static void DungeonGeneratorGenerate_Prefix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
                DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onBeforeDungeonGenerate.Invoke(RoundManager.Instance);
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SwitchPower")]
        [HarmonyPrefix]
        internal static void RoundManagerSwitchPower_Prefix(bool on)
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
                DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onPowerSwitchToggle.Invoke(on);
            if (LevelManager.CurrentExtendedLevel != null)
                LevelManager.CurrentExtendedLevel.levelEvents.onPowerSwitchToggle.Invoke(on);
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnScrapInLevel_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
                if (DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onSpawnedScrapObjects.HasListeners)
                    DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onSpawnedScrapObjects.Invoke(UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList());
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnSyncedProps")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnSyncedProps_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
                if (DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onSpawnedSyncedObjects.HasListeners)
                    DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onSpawnedSyncedObjects.Invoke(RoundManager.Instance.spawnedSyncedObjects);
        }

        private static EnemyVent cachedSelectedVent;
        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        internal static void RoundManagerSpawnEventFromVent_Prefix(EnemyVent vent)
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
                cachedSelectedVent = vent;
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyGameObject")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnEventFromVent_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null && cachedSelectedVent != null)
            {
                DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onEnemySpawnedFromVent.Invoke((cachedSelectedVent, RoundManager.Instance.SpawnedEnemies.Last()));
                cachedSelectedVent = null;
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnMapObjects_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                List<GameObject> mapObjects = new List<GameObject>();
                foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.selectableLevel.sceneName).GetRootGameObjects())
                    foreach (SpawnableMapObject randomMapObject in LevelManager.CurrentExtendedLevel.selectableLevel.spawnableMapObjects)
                        if (rootObject.name.Sanitized().Contains(randomMapObject.prefabToSpawn.name.Sanitized())) //To ensure were only getting the Dungeon relevant objects.
                            mapObjects.Add(rootObject);
                DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onSpawnedMapObjects.Invoke(mapObjects);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayerServerRpc")]
        [HarmonyPrefix]
        internal static void EntranceTeleportTeleportPlayerServerRpc_Prefix(EntranceTeleport __instance)
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
                if (__instance.isEntranceToBuilding == true)
                    DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onPlayerEnterDungeon.Invoke((__instance, player));
                else
                    DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onPlayerExitDungeon.Invoke((__instance, player));
            }

            if (LevelManager.CurrentExtendedLevel != null)
            {
                PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
                if (__instance.isEntranceToBuilding == true)
                    LevelManager.CurrentExtendedLevel.levelEvents.onPlayerEnterDungeon.Invoke((__instance, player));
                else
                    LevelManager.CurrentExtendedLevel.levelEvents.onPlayerExitDungeon.Invoke((__instance, player));
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(LungProp), "EquipItem")]
        [HarmonyPrefix]
        internal static void LungPropEquipItem_Postfix(LungProp __instance)
        {
            if (__instance.IsServer == true)
            {
                if (DungeonManager.CurrentExtendedDungeonFlow != null)
                    DungeonManager.CurrentExtendedDungeonFlow.dungeonEvents.onApparatusTaken.Invoke(__instance);
                if (LevelManager.CurrentExtendedLevel != null)
                    LevelManager.CurrentExtendedLevel.levelEvents.onApparatusTaken.Invoke(__instance);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(TimeOfDay), "GetDayPhase")]
        [HarmonyPostfix]
        internal static void TimeOfDayGetDayPhase_Postfix(DayMode __result)
        {
            if (previousDayMode == DayMode.None || previousDayMode != __result)
                LevelManager.CurrentExtendedLevel.levelEvents.onDayModeToggle.Invoke(__result);

            previousDayMode = __result;

        }
    }

    public class ExtendedEvent<T>
    {
        public delegate void ParameterEvent(T param);
        private event ParameterEvent onParameterEvent;
        public bool HasListeners => (Listeners != 0);
        public int Listeners { get; internal set; }
        public void Invoke(T param) { onParameterEvent?.Invoke(param); }
        public void AddListener(ParameterEvent listener) { onParameterEvent += listener; Listeners++; }
        public void RemoveListener(ParameterEvent listener) { onParameterEvent -= listener; Listeners--; }
    }

    public class ExtendedEvent
    {
        public delegate void Event();
        private event Event onEvent;
        public bool HasListeners => (Listeners != 0);
        public int Listeners { get; internal set; }
        public void Invoke() { onEvent?.Invoke(); }
        public void AddListener(Event listener) { onEvent += listener; Listeners++; }
        public void RemoveListener(Event listener) { onEvent -= listener; Listeners--; }
    }
}
