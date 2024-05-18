using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
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
        internal static DayMode previousDayMode = DayMode.None;
        internal static bool firedDawnEvent = false;
        ////////// Level Patches //////////

        internal static void InvokeExtendedEvent<T>(ExtendedEvent<T> extendedEvent, T eventParameter)
        {
            extendedEvent.Invoke(eventParameter);
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (LevelManager.CurrentExtendedLevel != null && LevelManager.CurrentExtendedLevel.IsLevelLoaded)
            {
                previousDayMode = DayMode.None;

                LevelManager.CurrentExtendedLevel.LevelEvents.onLevelLoaded.Invoke();
                LevelManager.GlobalLevelEvents.onLevelLoaded.Invoke();
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(StoryLog), "CollectLog")]
        [HarmonyPrefix]
        internal static void StoryLogCollectLog_Prefix(StoryLog __instance)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
            {
                LevelManager.CurrentExtendedLevel.LevelEvents.onStoryLogCollected.Invoke(__instance);
                LevelManager.GlobalLevelEvents.onStoryLogCollected.Invoke(__instance);
            }
        }
        /*
        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnRandomDaytimeEnemy")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnRandomDaytimeEnemy_Postfix(RoundManager __instance, GameObject __result)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
                if (__result != null && __result.TryGetComponent(out EnemyAI enemyAI))
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onDaytimeEnemySpawn.Invoke(enemyAI);
                    LevelManager.GlobalLevelEvents.onDaytimeEnemySpawn.Invoke(enemyAI);
                }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnRandomOutsideEnemy_Postfix(RoundManager __instance, GameObject __result)
        {
            if (LevelManager.CurrentExtendedLevel != null && __instance.IsServer)
                if (__result != null && __result.TryGetComponent(out EnemyAI enemyAI))
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onNighttimeEnemySpawn.Invoke(enemyAI);
                    LevelManager.GlobalLevelEvents.onNighttimeEnemySpawn.Invoke(enemyAI);
                }
        }*/




        ////////// Dungeon Patches //////////

        [HarmonyPriority(Patches.harmonyPriority + 1)] // +1 Because this needs to run after the Patch in Patches, second patch here for consistency.
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static void DungeonGeneratorGenerate_Prefix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onBeforeDungeonGenerate.Invoke(Patches.RoundManager);
                DungeonManager.GlobalDungeonEvents.onBeforeDungeonGenerate.Invoke(Patches.RoundManager);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SwitchPower")]
        [HarmonyPrefix]
        internal static void RoundManagerSwitchPower_Prefix(bool on)
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPowerSwitchToggle.Invoke(on);
                DungeonManager.GlobalDungeonEvents.onPowerSwitchToggle.Invoke(on);
            }
            if (LevelManager.CurrentExtendedLevel != null)
            {
                LevelManager.CurrentExtendedLevel.LevelEvents.onPowerSwitchToggle.Invoke(on);
                LevelManager.GlobalLevelEvents.onPowerSwitchToggle.Invoke(on);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnScrapInLevel_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                List<GrabbableObject> scrap = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList();
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onSpawnedScrapObjects.Invoke(scrap);
                DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.Invoke(scrap);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnSyncedProps")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnSyncedProps_Postfix()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onSpawnedSyncedObjects.Invoke(Patches.RoundManager.spawnedSyncedObjects);
                DungeonManager.GlobalDungeonEvents.onSpawnedSyncedObjects.Invoke(Patches.RoundManager.spawnedSyncedObjects);
            }
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
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onEnemySpawnedFromVent.Invoke((cachedSelectedVent, Patches.RoundManager.SpawnedEnemies.Last()));
                DungeonManager.GlobalDungeonEvents.onEnemySpawnedFromVent.Invoke((cachedSelectedVent, Patches.RoundManager.SpawnedEnemies.Last()));
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
                foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.SelectableLevel.sceneName).GetRootGameObjects())
                    foreach (SpawnableMapObject randomMapObject in LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects)
                        if (rootObject.name.Sanitized().Contains(randomMapObject.prefabToSpawn.name.Sanitized())) //To ensure were only getting the Dungeon relevant objects.
                            mapObjects.Add(rootObject);
                DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onSpawnedMapObjects.Invoke(mapObjects);
                DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.Invoke(mapObjects);
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "OnShipLandedMiscEvents")]
        [HarmonyPrefix]
        internal static void StartOfRoundOnShipLandedMiscEvents_Prefix()
        {
            LevelManager.CurrentExtendedLevel.LevelEvents.onShipLand.Invoke();
            LevelManager.GlobalLevelEvents.onShipLand.Invoke();
            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onShipLand.Invoke();
            DungeonManager.GlobalDungeonEvents.onShipLand.Invoke();
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "ShipLeave")]
        [HarmonyPrefix]
        internal static void StartOfRoundShipLeave_Prefix()
        {
            LevelManager.CurrentExtendedLevel.LevelEvents.onShipLeave.Invoke();
            LevelManager.GlobalLevelEvents.onShipLeave.Invoke();
            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onShipLeave.Invoke();
            DungeonManager.GlobalDungeonEvents.onShipLeave.Invoke();
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayerServerRpc")]
        [HarmonyPrefix]
        internal static void EntranceTeleportTeleportPlayerServerRpc_Prefix(EntranceTeleport __instance, int playerObj)
        {
            if (__instance.IsHost == false) return;

            if (DungeonManager.CurrentExtendedDungeonFlow != null)
            {
                PlayerControllerB player = Patches.StartOfRound.allPlayerScripts[playerObj];
                if (__instance.isEntranceToBuilding == true)
                {
                    DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerEnterDungeon.Invoke((__instance, player));
                    DungeonManager.GlobalDungeonEvents.onPlayerEnterDungeon.Invoke((__instance, player));
                }
                else
                {
                    DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerExitDungeon.Invoke((__instance, player));
                    DungeonManager.GlobalDungeonEvents.onPlayerExitDungeon.Invoke((__instance, player));
                }
            }

            if (LevelManager.CurrentExtendedLevel != null)
            {
                PlayerControllerB player = Patches.StartOfRound.allPlayerScripts[playerObj];
                if (__instance.isEntranceToBuilding == true)
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onPlayerEnterDungeon.Invoke((__instance, player));
                    LevelManager.GlobalLevelEvents.onPlayerEnterDungeon.Invoke((__instance, player));

                }
                else
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onPlayerExitDungeon.Invoke((__instance, player));
                    LevelManager.GlobalLevelEvents.onPlayerExitDungeon.Invoke((__instance, player));
                }
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
                {
                    DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onApparatusTaken.Invoke(__instance);
                    DungeonManager.GlobalDungeonEvents.onApparatusTaken.Invoke(__instance);
                }
                if (LevelManager.CurrentExtendedLevel != null)
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onApparatusTaken.Invoke(__instance);
                    LevelManager.GlobalLevelEvents.onApparatusTaken.Invoke(__instance);
                }
            }
        }

        [HarmonyPriority(Patches.harmonyPriority)]
        [HarmonyPatch(typeof(TimeOfDay), "GetDayPhase")]
        [HarmonyPostfix]
        internal static void TimeOfDayGetDayPhase_Postfix(DayMode __result)
        {
            if (previousDayMode == DayMode.None || previousDayMode != __result)
            {
                LevelManager.CurrentExtendedLevel.LevelEvents.onDayModeToggle.Invoke(__result);
                LevelManager.GlobalLevelEvents.onDayModeToggle.Invoke (__result);
            }

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
