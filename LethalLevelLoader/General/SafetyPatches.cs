using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace LethalLevelLoader
{
    internal class SafetyPatches
    {
        internal const int harmonyPriority = 250;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
        [HarmonyPrefix]
        internal static void StartOfRoundChangeLevel_Prefix(ref int levelID)
        {
           /* if (levelID >= Patches.StartOfRound.levels.Length)
            {
                DebugHelper.LogWarning("Lethal Company attempted to load a saved current level that has not yet been loaded");
                DebugHelper.LogWarning(levelID + " / " + (Patches.StartOfRound.levels.Length));
                LevelManager.invalidSaveLevelID = levelID;
                levelID = 0;
            }*/
        }

        static List<SpawnableMapObject> tempoarySpawnableMapObjectList = new List<SpawnableMapObject>();

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPrefix]
        internal static bool RoundManagerSpawnMapObjects_Prefix()
        {
            if (GameObject.FindGameObjectsWithTag("MapPropsContainer") == null)
            {
                DebugHelper.LogError("ExtendedLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " Is Missing A \"MapPropsContainer\" Tagged GameObject. \nPreventing Spawning Of Interior RandomMapObjects To Prevent Gamebreaking Error!", DebugType.User);
                return (false);
            }

            RandomMapObject[] array = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            if (array.Length == 0)
            {
                DebugHelper.LogError("ExtendedDungeonFlow: " + DungeonManager.CurrentExtendedDungeonFlow.DungeonName + " Spawned 0 RandomMapObjects. \nPreventing Spawning Of Interior RandomMapObjects To Prevent Gamebreaking Error!", DebugType.User);
                return (false);
            }

            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);

            List<GameObject> uniqueMapObjectSpawnablePrefabs = new List<GameObject>();
            foreach (RandomMapObject randomMapObject in array)
                foreach (GameObject spawnablePrefab in randomMapObject.spawnablePrefabs)
                    if (spawnablePrefab != null && !uniqueMapObjectSpawnablePrefabs.Contains(spawnablePrefab))
                        uniqueMapObjectSpawnablePrefabs.Add(spawnablePrefab);

            List<GameObject> orphanMapObjectSpawnablePrefabs = new List<GameObject>(uniqueMapObjectSpawnablePrefabs);
            foreach (SpawnableMapObject spawnableMapObject in LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects)
                if (spawnableMapObject.prefabToSpawn != null && orphanMapObjectSpawnablePrefabs.Contains(spawnableMapObject.prefabToSpawn))
                    orphanMapObjectSpawnablePrefabs.Remove(spawnableMapObject.prefabToSpawn);

            foreach (GameObject orphanedMapObject in orphanMapObjectSpawnablePrefabs)
            {
                DebugHelper.Log("ExtendedDungeonFlow: " + DungeonManager.CurrentExtendedDungeonFlow.DungeonName + " Spawned RandomMapObject Prefab: " + orphanedMapObject.name + " That Was Not Found In Current ExtendedLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + ". Temporarily Injecting RandomMapObject Into Level!", DebugType.Developer);
                SpawnableMapObject spawnableMapObject = new SpawnableMapObject();
                AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0));
                spawnableMapObject.prefabToSpawn = orphanedMapObject;
                spawnableMapObject.numberToSpawn = curve;
                spawnableMapObjects.Add(spawnableMapObject);
                tempoarySpawnableMapObjectList.Add(spawnableMapObject);
            }
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();

            return (true);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnMapObjects_Postfix()
        {
            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);
            foreach (SpawnableMapObject spawnableMapObject in tempoarySpawnableMapObjectList)
                spawnableMapObjects.Remove(spawnableMapObject);
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();
            tempoarySpawnableMapObjectList.Clear();
        }


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        internal static bool RoundManagerSpawnScrapInLevel_Prefix()
        {
            List<SpawnableItemWithRarity> invalidSpawnableItemWithRarity = new List<SpawnableItemWithRarity>();
            foreach (SpawnableItemWithRarity spawnableScrap in LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap)
                if (spawnableScrap.spawnableItem == null || spawnableScrap.rarity == 0)
                    invalidSpawnableItemWithRarity.Add(spawnableScrap);

            if (invalidSpawnableItemWithRarity.Count != 0)
                DebugHelper.LogError("Removed: " + invalidSpawnableItemWithRarity.Count + " SpawnableItemWithRarities From CurrentLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " Due To Invalid Properties To Prevent Errors.", DebugType.User);
            foreach (SpawnableItemWithRarity invalidItem in invalidSpawnableItemWithRarity)
                LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap.Remove(invalidItem);

            if (LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap.Count == 0)
            {
                DebugHelper.LogError("Current ExtendedLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " Requested 0 SpawnableScrap, Returning Early To Prevent Errors", DebugType.User);
                return (false);
            }

            return (true);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(TimeOfDay), "SetWeatherBasedOnVariables")]
        [HarmonyPrefix]
        internal static void TimeOfDaySetWeatherBasedOnVariables_Prefix(TimeOfDay __instance)
        {
            if (__instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                if ((int)__instance.currentWeatherVariable >= (int)__instance.currentWeatherVariable2)
                {
                    DebugHelper.LogError("TimeOfDay Foggy CurrentWeatherVariable (Int) Was Equal Or Higher Than CurrentWeatherVariable2 (Int). Resetting For Safetey!", DebugType.User);
                    __instance.currentWeatherVariable = 3f;
                    __instance.currentWeatherVariable2 = 10f;
                }
                else if ((int)__instance.currentWeatherVariable <= 0 || (int)__instance.currentWeatherVariable2 <= 0)
                {
                    DebugHelper.LogError("TimeOfDay Foggy CurrentWeatherVariable (Int) And/Or CurrentWeatherVariable2 (Int) Were 0. Resetting For Safetey!", DebugType.User);
                    __instance.currentWeatherVariable = 3f;
                    __instance.currentWeatherVariable2 = 10f;
                }
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(LethalLib.Modules.Dungeon), "RoundManager_Start")]
        [HarmonyPrefix]
        internal static bool Dungeon_Start_Prefix(Action<RoundManager> orig, RoundManager self)
        {
            DebugHelper.LogWarning("Disabling LethalLib Dungeon.RoundManager_Start() Function To Prevent Conflicts", DebugType.User);
            orig(self);
            return (false);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(LethalLib.Modules.Dungeon), "RoundManager_GenerateNewFloor")]
        [HarmonyPrefix]
        internal static bool Dungeon_GenerateNewFloor_Prefix(Action<RoundManager> orig, RoundManager self)
        {
            DebugHelper.LogWarning("Disabling LethalLib Dungeon.RoundManager_GenerateNewFloor() Function To Prevent Conflicts", DebugType.User);
            orig(self);
            return (false);
        }
    }
}
