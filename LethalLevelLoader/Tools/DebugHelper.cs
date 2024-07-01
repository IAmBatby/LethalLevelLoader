using BepInEx.Logging;
using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = System.Random;
using Log = UnityEngine.Debug;

namespace LethalLevelLoader
{
    internal static class DebugHelper
    {
        public static string logAuthor = "Batby";

        public static Dictionary<ExtendedLevel, ExtendedLevelLogReport> extendedLevelLogReports = new Dictionary<ExtendedLevel, ExtendedLevelLogReport>();
        public static Dictionary<ExtendedDungeonFlow, ExtendedLevelLogReport> extendedDungeonFlowLogReports = new Dictionary<ExtendedDungeonFlow, ExtendedLevelLogReport>();

        public static void Log(string log, DebugType debugType)
        {
            if (!string.IsNullOrEmpty(log) && (int)Settings.debugType >= (int)debugType)
            {
                string logString = log;
                if (Plugin.logger != null)
                    Plugin.logger.LogInfo(logString);
                else
                    UnityEngine.Debug.Log("LethalLevelLoader Fallback Logger: " + logString);
            }
        }

        public static void LogWarning(string log, DebugType debugType)
        {
            if (!string.IsNullOrEmpty(log) && (int)Settings.debugType >= (int)debugType)
            {
                string logString = log;
                if (Plugin.logger != null)
                    Plugin.logger.LogWarning(logString);
                else
                    UnityEngine.Debug.LogWarning("LethalLevelLoader Fallback Logger: " + logString);
            }
        }

        public static void LogError(string log, DebugType debugType)
        {
            if (!string.IsNullOrEmpty(log) && (int)Settings.debugType >= (int)debugType)
            {
                string logString = log;
                if (Plugin.logger != null)
                    Plugin.logger.LogError(logString);
                else
                    UnityEngine.Debug.LogError("LethalLevelLoader Fallback Logger: " + logString);
            }
        }

        public static void LogError(Exception exception, DebugType debugType)
        {
            if (exception != null && (int)Settings.debugType >= (int)debugType)
            {
                if (Plugin.logger != null)
                    Plugin.logger.LogError(exception);
                else
                    UnityEngine.Debug.LogError("LethalLevelLoader Fallback Logger: " + exception);
            }
        }

        public static void DebugTerminalKeyword(TerminalKeyword terminalKeyword)
        {
            if (terminalKeyword != null)
            {
                string logString = "Info For (" + terminalKeyword.word + ") TerminalKeyword!" + "\n" + "\n";
                logString += "Word: " + terminalKeyword.word + "\n";
                logString += "isVerb?: " + terminalKeyword.isVerb + "\n";
                logString += "CompatibleNouns :" + "\n";
                if (terminalKeyword.compatibleNouns != null)
                {
                    foreach (CompatibleNoun compatibleNoun in terminalKeyword.compatibleNouns)
                    {
                        if (compatibleNoun != null && compatibleNoun.noun != null && compatibleNoun.result != null)
                            logString += compatibleNoun.noun.word + " | " + compatibleNoun.result + "\n";
                        else
                            logString += "Could not debug CompatibleNoun as it was null!" + "\n";
                    }
                }
                logString += "SpecialKeywordResult: " + terminalKeyword.specialKeywordResult + "\n";
                logString += "AccessTerminalObjects?: " + terminalKeyword.accessTerminalObjects + "\n";
                if (terminalKeyword.defaultVerb != null && terminalKeyword.defaultVerb.word != null)
                    logString += "DefaultVerb: " + terminalKeyword.defaultVerb.word + "\n";
                else
                    logString += "Could not debug DefaultVerb as it was null!" + "\n";
                Log(logString + "\n" + "\n", DebugType.Developer);
            }
            else
                Log("Could not debug TerminalKeyword as it was null!", DebugType.Developer);
        }

        public static void DebugTerminalNode(TerminalNode terminalNode)
        {
            string logString = "Info For (" + terminalNode.name + ") TerminalNode!" + "\n" + "\n";
            logString += "Display Text: " + terminalNode.displayText + "\n";
            logString += "Terminal Event: " + terminalNode.terminalEvent + "\n";
            logString += "Accept Anything?: " + terminalNode.acceptAnything + "\n";
            logString += "Override Options?: " + terminalNode.overrideOptions + "\n";
            logString += "Display Planet Info (LevelID): " + terminalNode.displayPlanetInfo + "\n";
            logString += "Buy Reroute To Moon (LevelID): " + terminalNode.buyRerouteToMoon + "\n";
            logString += "Is Confirmation Node?: " + terminalNode.isConfirmationNode + "\n";
            logString += "Terminal Options (CompatibleNouns) :" + "\n";

            foreach (CompatibleNoun compatibleNoun in terminalNode.terminalOptions)
                logString += compatibleNoun.noun + " | " + compatibleNoun.result + "\n";

            Log(logString + "\n" + "\n", DebugType.Developer);
        }

        public static void DebugInjectedLevels()
        {
            string logString = "Injected Levels List: " + "\n" + "\n";

            int counter = 0;
            if (Patches.StartOfRound != null)
            {
                foreach (SelectableLevel level in Patches.StartOfRound.levels)
                {
                    logString += counter + ". " + level.PlanetName + " (" + level.levelID + ") " + "\n";
                    counter++;
                }

                logString += "Current Level Is: " + Patches.StartOfRound.currentLevel.PlanetName + " (" + Patches.StartOfRound.currentLevel.levelID + ") " + "\n";
            }

            Log(logString + "\n" + "\n", DebugType.Developer);
        }



        public static void DebugAllLevels()
        {
            string logString = "All Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                logString += extendedLevel.SelectableLevel.PlanetName + " (" + extendedLevel.SelectableLevel.levelID + ") " + "\n";

            Log(logString + "\n", DebugType.Developer);
        }

        public static void DebugVanillaLevels()
        {
            string logString = "Vanilla Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaExtendedLevels)
                logString += extendedLevel.SelectableLevel.PlanetName + " (" + extendedLevel.SelectableLevel.levelID + ") " + "\n";

            Log(logString + "\n", DebugType.Developer);
        }

        public static void DebugCustomLevels()
        {
            string logString = "Custom Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in PatchedContent.CustomExtendedLevels)
                logString += extendedLevel.SelectableLevel.PlanetName + " (" + extendedLevel.SelectableLevel.levelID + ") " + "\n";

            Log(logString + "\n", DebugType.Developer);
        }

        public static void DebugScrapedVanillaContent()
        {
            Log("Obtained (" + OriginalContent.SelectableLevels.Count + " / 9) Vanilla SelectableLevel References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.DungeonFlows.Count + " / 4) Vanilla DungeonFlow References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.Items.Count + " / 68) Vanilla Item References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.ItemGroups.Count + " / 3) Vanilla Item Group References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.Enemies.Count + " / 20) Vanilla Enemy References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.SpawnableOutsideObjects.Count + " / 11) Vanilla Outside Object References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.SpawnableMapObjects.Count + " / 2) Vanilla Inside Object References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.AudioMixers.Count + " / 2) Vanilla Audio Mixer References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.AudioMixerGroups.Count + " / 9) Vanilla Audio Mixing Group References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.AudioMixerSnapshots.Count + " / 6) Vanilla Audio Mixing Snapshot References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.LevelAmbienceLibraries.Count + " / 3) Vanilla Ambience Library References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.ReverbPresets.Count + " / 8) Vanilla Reverb References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.TerminalKeywords.Count + " / 121) Vanilla Terminal Keyword References", DebugType.Developer);

            Log("Obtained (" + OriginalContent.TerminalNodes.Count + " / 186) Vanilla Terminal Node References", DebugType.Developer);

            foreach (TerminalNode terminalNode in Resources.FindObjectsOfTypeAll(typeof(TerminalNode)))
                if (!OriginalContent.TerminalNodes.Contains(terminalNode))
                    Log("Missing Terminal Node: " + terminalNode.name, DebugType.Developer);
        }

        public static void DebugAudioMixerGroups()
        {

        }


        public static void DebugSelectableLevelReferences(ExtendedLevel extendedLevel)
        {
            string logString = "Logging SelectableLevel References For Moon: " + extendedLevel.NumberlessPlanetName + " (" + extendedLevel.ContentType.ToString() + ")." + "\n";

            logString += "Inside Enemies" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.SelectableLevel.Enemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";

            logString += "Outside Enemies (Nighttime)" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.SelectableLevel.OutsideEnemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";

            logString += "Outside Enemies (daytime)" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.SelectableLevel.DaytimeEnemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";


            Log(logString + "\n", DebugType.Developer);
        }

        public static void DebugDungeonFlows(List<DungeonFlow> dungeonFlowList)
        {
            string debugString = "Dungen Flow Report: " + "\n" + "\n";

            foreach (DungeonFlow dungeonFlow in dungeonFlowList)
                debugString += dungeonFlow.name + "\n";
        }

        public static string GetDungeonFlowsLog(List<DungeonFlow> dungeonFlowList)
        {
            string returnString = string.Empty;

            foreach (DungeonFlow dungeonFlow in dungeonFlowList)
                returnString += dungeonFlow.name + "\n";

            return (returnString);
        }

        public static void DebugAllExtendedDungeons()
        {
            string debugString = "All ExtendedDungeons: " + "\n" + "\n";

            foreach (ExtendedDungeonFlow dungeonFlow in PatchedContent.ExtendedDungeonFlows)
                debugString += dungeonFlow.DungeonFlow.name;

            Log(debugString, DebugType.Developer);

            debugString = "Vanilla ExtendedDungeons: " + "\n" + "\n";

            foreach (ExtendedDungeonFlow dungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
                debugString += dungeonFlow.DungeonFlow.name;

            Log(debugString, DebugType.Developer);

            debugString = "Custom ExtendedDungeons: " + "\n" + "\n";

            foreach (ExtendedDungeonFlow dungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                debugString += dungeonFlow.DungeonFlow.name;

            Log(debugString, DebugType.Developer);
        }

        /*[HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
        [HarmonyPrefix]
        public static void SetPlanetsWeather_Prefix(StartOfRound __instance, int connectedPlayersOnServer)
        {
            DebugPlanetWeatherRandomisation(connectedPlayersOnServer, __instance.levels.ToList());
        }*/

        public static void DebugPlanetWeatherRandomisation(int players, List<SelectableLevel> selectableLevelsList)
        {
            StartOfRound startOfRound = Patches.StartOfRound;

            List<SelectableLevel> selectableLevels = new List<SelectableLevel>(selectableLevelsList);

            //Recreate Weather Random Stuff

            foreach (SelectableLevel selectableLevel in selectableLevels)
                selectableLevel.currentWeather = LevelWeatherType.None;

            Random weatherRandom = new Random(startOfRound.randomMapSeed + 31);

            float playerRandomFloat = 1f;

            if (players + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
                playerRandomFloat = (float)weatherRandom.Next(15, 25) / 10f;

            int randomPlanetWeatherCurve = Mathf.Clamp((int)(Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)weatherRandom.NextDouble()) * playerRandomFloat, 0f, 1f) * (float)selectableLevels.Count), 0, selectableLevels.Count);

            //Debug Logging

            string debugString = string.Empty;
            debugString += "Start Of SetPlanetWeather() Prefix." + "\n";
            debugString += "Planet Weather Being Set! Details Below;" + "\n" + "\n";
            debugString += "RandomMapSeed Is: " + startOfRound.randomMapSeed + "\n";
            debugString += "Planet Random Is: " + weatherRandom + "\n";
            debugString += "Player Random Is: " + playerRandomFloat + "\n";
            debugString += "Result From PlanetWeatherRandomCurve Is: " + randomPlanetWeatherCurve + "\n";
            debugString += "All SelectableLevels In StartOfRound: " + "\n" + "\n";


            foreach (SelectableLevel selectableLevel in selectableLevels)
            {
                debugString += selectableLevel.PlanetName + " | " + selectableLevel.currentWeather + " | " + selectableLevel.overrideWeather + "\n";
                foreach (RandomWeatherWithVariables randomWeather in selectableLevel.randomWeathers)
                    debugString += randomWeather.weatherType.ToString() + " | " + randomWeather.weatherVariable + " | " + randomWeather.weatherVariable2 + "\n";

                debugString += "\n";
            }

            debugString += "SelectableLevels Chosen Using Random Variables Should Be: " + "\n" + "\n";

            for (int j = 0; j < randomPlanetWeatherCurve; j++)
            {
                SelectableLevel selectableLevel = selectableLevels[weatherRandom.Next(0, selectableLevels.Count)];
                debugString += "SelectableLevel Chosen! Planet Name Is: " + selectableLevel.PlanetName;
                if (selectableLevel.randomWeathers != null && selectableLevel.randomWeathers.Length != 0)
                {
                    int randomSelection = weatherRandom.Next(0, selectableLevel.randomWeathers.Length);
                    debugString += " --- Selected For Weather Change! Setting WeatherType From: " + selectableLevel.currentWeather + " To: " + selectableLevel.randomWeathers[randomSelection].weatherType + "\n";
                    debugString += "          Random Selection Results Were: " + randomSelection + " (Range: 0 - " + selectableLevel.randomWeathers.Length + ") Level RandomWeathers Choices Were: " + "\n" + "          ";

                    int index = 0;
                    foreach (RandomWeatherWithVariables weatherType in selectableLevel.randomWeathers)
                    {
                        debugString += index + " . - " + weatherType.weatherType + ", ";
                        index++;
                    }
                    debugString += "\n" + "\n";
                }
                else
                    debugString += "\n";
                selectableLevels.Remove(selectableLevel);
            }

            debugString += "End Of SetPlanetWeather() Prefix." + "\n" + "\n";

            DebugHelper.Log(debugString, DebugType.Developer);
        }

        /*[HarmonyPatch(typeof(StartOfRound), "SetTimeAndPlanetToSavedSettings")]
        [HarmonyPrefix]
        public static void SetTimeAndPlanetToSavedSettings_Prefix()
        {
            DebugHelper.Log("SaveGameValues Prefix.");
            DebugHelper.Log("Current Level ID: " + Patches.StartOfRound.currentLevelID);
            DebugHelper.Log("Current Level List Count: " + Patches.StartOfRound.levels.Length);
            DebugHelper.Log("Current Level From ID: " + Patches.StartOfRound.levels[Patches.StartOfRound.currentLevelID]);
        }

        [HarmonyPatch(typeof(GameNetworkManager), "SaveGameValues")]
        [HarmonyPrefix]
        public static void SaveGameValues_Prefix()
        {
            DebugHelper.Log("SaveGameValues Prefix.");
            DebugHelper.Log("Current Level ID: " + Patches.StartOfRound.currentLevelID);
            DebugHelper.Log("Current Level List Count: " + Patches.StartOfRound.levels.Length);
            DebugHelper.Log("Current Level From ID: " + Patches.StartOfRound.levels[Patches.StartOfRound.currentLevelID]);
        }*/


        /*[HarmonyPatch(typeof(EnemyAI), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        public static void DebugEnemySpawn(EnemyAI __instance)
        {
            EnemyAI enemy = __instance;
            string enemyName = __instance.enemyType.enemyName;

            Log(enemyName + " Spawned At: " + enemy.transform.position);
            Log(enemyName + " IsOnNavMesh = : " + enemy.agent.isOnNavMesh);
            if (enemy.agent.isOnNavMesh == true)
                Log(enemyName + "NavMeshSurface Is: " + enemy.agent.navMeshOwner.name);
        }

        public static void DebugNetworkComponents(Scene scene)
        {
            List<NetworkObject> networkObjects = new List<NetworkObject>();
            List<NetworkBehaviour> networkBehaviours = new List<NetworkBehaviour>();
            Log("Starting Debug Network Components, Scene Is: " + scene.name);
            string debugString = "Network Components Report." + "\n";
            debugString += "Current Level Being Reported On Is: " + Patches.RoundManager.currentLevel.PlanetName;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (NetworkObject networkObject in rootObject.GetComponentsInChildren<NetworkObject>())
                    networkObjects.Add(networkObject);

                foreach (NetworkBehaviour networkBehaviour in rootObject.GetComponentsInChildren<NetworkBehaviour>())
                    networkBehaviours.Add(networkBehaviour);
            }

            debugString = "NetworkObjects Report" + "\n" + "\n";
            foreach (NetworkObject networkObject in networkObjects)
            {
                debugString += "NetworkObject Name: " + networkObject.gameObject.name + ", IsSpawned: " + networkObject.IsSpawned + "\n";
                debugString += "NetworkBehaviours: " + "\n";
                foreach (NetworkBehaviour networkBehaviour in networkObject.ChildNetworkBehaviours)
                    debugString += networkBehaviour.gameObject.name + ",";
                debugString += "\n" + "\n";
            }
            Log(debugString);

            debugString = "NetworkBehaviourss Report" + "\n" + "\n";
            foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
            {
                debugString += "NetworkBehaviour Name: " + networkBehaviour.gameObject.name + ", IsSpawned: " + networkBehaviour.IsSpawned + "\n";
                if (networkBehaviour.NetworkObject != null)
                    debugString += "NetworkObject: " + networkBehaviour.NetworkObject.gameObject.name + "\n";
                else
                    debugString += "NetworkObject: (Null)" + "\n";
                debugString += "\n" + "\n";
            }

            Log(debugString);
        }*/


        /*[HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetAndSync")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static void SwitchRadarTargetAndSync(ManualCameraRenderer __instance, int switchToIndex)
        {
            DebugHelper.Log("Switching Radar Target! ID: " + switchToIndex + " Target: " + __instance.radarTargets[switchToIndex].name);
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "RemoveTargetFromRadar")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static void SwitchRadarTargetAndSync(ManualCameraRenderer __instance, Transform removeTransform)
        {
            DebugHelper.Log("Removing Radar Target! Removed Target: " + removeTransform.gameObject.name);
        }*/

        internal static void DebugExtendedLevelGroups(List<MoonsCataloguePage> extendedLevelGroups)
        {
            /*DebugHelper.Log("Debugging ExtendedLevelGroups");

            int counter = 1;
            foreach (MoonsCataloguePage group in extendedLevelGroups)
            {
                DebugHelper.Log("Group " + counter + " / " + extendedLevelGroups.Count);
                int counter2 = 1;
                foreach (ExtendedLevel level in group.extendedLevelsList)
                {
                    DebugHelper.Log("Group Level " + counter2 + " / " + group.extendedLevelsList.Count + " : " + level.NumberlessPlanetName);
                    counter2++;
                }
                counter++;
            }*/
        }

        public static void DebugExtendedDungeonFlowTiles(ExtendedDungeonFlow extendedDungeonFlow)
        {
            string debugString = "Logging All Tiles In DungeonFlow: " + extendedDungeonFlow.DungeonName + "\n";
            foreach (Tile tile in extendedDungeonFlow.DungeonFlow.GetTiles())
                debugString += tile.gameObject.name + "\n";
            DebugHelper.Log(debugString, DebugType.Developer);
        }

        public static void DebugExtendedDungeonSpawnSyncedObjects(ExtendedDungeonFlow extendedDungeonFlow)
        {
            string debugString = "Logging All SpawnSyncedObjects In DungeonFlow: " + extendedDungeonFlow.DungeonName + "\n";
            foreach (SpawnSyncedObject spawnSyncedObject in extendedDungeonFlow.DungeonFlow.GetSpawnSyncedObjects())
                debugString += spawnSyncedObject.gameObject.name + " | " + spawnSyncedObject.spawnPrefab.gameObject.name + "\n";
            DebugHelper.Log(debugString, DebugType.Developer);
        }

        public static void DebugExtendedDungeonFlowRandomMapObjects(ExtendedDungeonFlow extendedDungeonFlow)
        {
            string debugString = "Logging All RandomMapObjects In DungeonFlow: " + extendedDungeonFlow.DungeonName + "\n";
            foreach (RandomMapObject randomMapObjectObject in extendedDungeonFlow.DungeonFlow.GetRandomMapObjects())
                debugString += randomMapObjectObject.gameObject.name + " | " + randomMapObjectObject.spawnablePrefabs[0].gameObject.name + "\n";
            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static void DebugMoonsCataloguePage(MoonsCataloguePage moonsCataloguePage)
        {
            string debugString = "Finished Refreshing Current Moons Catalogue, Results Are" + "\n";

            foreach (ExtendedLevelGroup extendedLevelGroup in moonsCataloguePage.ExtendedLevelGroups)
            {
                debugString += "\n";
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    debugString += moonsCataloguePage.ExtendedLevelGroups.IndexOf(extendedLevelGroup) + " - " + extendedLevel.NumberlessPlanetName + "\n";
            }

            Log(debugString, DebugType.Developer);
        }

        internal static void DebugStringToStringWithRarityListParser(string inputString)
        {
            string debugString = "Debugging String To StringWithRarity List Parser." + "\n";
            debugString += "Input String Is: (" + inputString + ")" + "\n";

            List<StringWithRarity> stringWithRarities = ConfigHelper.ConvertToStringWithRarityList(inputString, Vector2.zero);

            debugString += "Parsed StringWithRarities; " + "\n";
            bool foundMatchingLevel;
            foreach (StringWithRarity pair in stringWithRarities)
            {
                debugString += "String: " + pair.Name + " , Rarity: " + pair.Rarity;
                foundMatchingLevel = false;
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.NumberlessPlanetName.ToLower().Contains(pair.Name.ToLower()) || pair.Name.ToLower().Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                    {
                        debugString += " | Found Loaded ExtendedLevel: " + extendedLevel.SelectableLevel.PlanetName + " From Parsed String: " + pair.Name + "\n";
                        foundMatchingLevel = true;
                    }
                if (foundMatchingLevel == false)
                    debugString += "\n";
            }

            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static void DebugStringToVector2WithRarityListParser(string inputString)
        {
            string debugString = "Debugging String To Vector2WithRarity List Parser." + "\n";
            debugString += "Input String Is: (" + inputString + ")" + "\n";

            List<Vector2WithRarity> stringWithRarities = ConfigHelper.ConvertToVector2WithRarityList(inputString, Vector2.zero);

            debugString += "Parsed Vector2WithRarities; " + "\n";
            foreach (Vector2WithRarity pair in stringWithRarities)
            {
                debugString += "Min: " + pair.Min + " , Max: " + pair.Max + " , Rarity: " + pair.Rarity + "\n";
            }

            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static void DebugStringToSpawnableEnemiesWithRarityListParser(string inputString)
        {
            string debugString = "Debugging String To SpawnableEnemyWithRarity List Parser." + "\n";
            debugString += "Input String Is: (" + inputString + ")" + "\n";

            List<SpawnableEnemyWithRarity> stringWithRarities = ConfigHelper.ConvertToSpawnableEnemyWithRarityList(inputString, Vector2.zero);

            debugString += "Parsed SpawnableEnemyWithRarities; " + "\n";
            foreach (SpawnableEnemyWithRarity pair in stringWithRarities)
            {
                if (pair.enemyType != null)
                    debugString += "Enemy Name: " + pair.enemyType.enemyName + " , Rarity: " + pair.rarity + "\n";
                else
                    debugString += "EnemyType Was Null, Skipping!" + "\n";
            }

            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static void DebugAudioAssets()
        {
            DebugHelper.Log("Debugging Vanilla Audio Assets", DebugType.Developer);

            foreach (AudioMixer audioMixer in OriginalContent.AudioMixers)
                DebugHelper.Log("Vanilla AudioMixer: " + audioMixer.name, DebugType.Developer);

            foreach (AudioMixerGroup audioMixerGroup in OriginalContent.AudioMixerGroups)
                DebugHelper.Log("Vanilla AudioMixerGroup: " + audioMixerGroup.name + " | " + audioMixerGroup.audioMixer.name, DebugType.Developer);

            foreach (AudioMixerSnapshot audioMixerSnapshot in OriginalContent.AudioMixerSnapshots)
                DebugHelper.Log("Vanilla AudioMixerSnapshot: " + audioMixerSnapshot.name + " | " + audioMixerSnapshot.audioMixer.name, DebugType.Developer);


            DebugHelper.Log("Debugging Custom Audio Assets", DebugType.Developer);

            foreach (AudioMixer audioMixer in PatchedContent.AudioMixers)
                DebugHelper.Log("Custom AudioMixer: " + audioMixer.name, DebugType.Developer);

            foreach (AudioMixerGroup audioMixerGroup in PatchedContent.AudioMixerGroups)
                DebugHelper.Log("Custom AudioMixerGroup: " + audioMixerGroup.name + " | " + audioMixerGroup.audioMixer.name, DebugType.Developer);

            foreach (AudioMixerSnapshot audioMixerSnapshot in PatchedContent.AudioMixerSnapshots)
                DebugHelper.Log("Custom AudioMixerSnapshot: " + audioMixerSnapshot.name + " | " + audioMixerSnapshot.audioMixer.name, DebugType.Developer);

        }

        public static void DebugSpawnScrap(ExtendedLevel extendedLevel)
        {
            foreach (SpawnableItemWithRarity scrap in extendedLevel.SelectableLevel.spawnableScrap)
            {
                if (scrap.spawnableItem.spawnPrefab != null)
                    DebugHelper.Log(extendedLevel.SelectableLevel.spawnableScrap.IndexOf(scrap) + " - " + scrap.spawnableItem.name + scrap.spawnableItem.spawnPrefab.name, DebugType.Developer);
                else
                    DebugHelper.Log(extendedLevel.SelectableLevel.spawnableScrap.IndexOf(scrap) + " - " + scrap.spawnableItem.name + "(Null)", DebugType.Developer);
            }
        }

        public static void DebugExtendedMod(ExtendedMod extendedMod)
        {
            string debugString = "Debug Report For ExtendedMod: " + extendedMod.ModName + " by " + extendedMod.AuthorName + "\n";

            debugString += "\nExtendedContents: Count - " + extendedMod.ExtendedContents.Count + "\n";
            foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                debugString += "\n" + extendedContent.name + " (" + extendedContent.GetType().Name + ")";

            Log(debugString + "\n", DebugType.Developer);
        }

        public static void DebugAllContentTags()
        {
            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods.Concat(new List<ExtendedMod>() { PatchedContent.VanillaMod }))
            {
                List<ContentTag> foundContentTags = new List<ContentTag>();
                Dictionary<ContentTag, List<ExtendedContent>> foundContentTagsDict = new Dictionary<ContentTag, List<ExtendedContent>>();
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                    foreach (ContentTag contentTag in extendedContent.ContentTags)
                    {
                        if (foundContentTagsDict.TryGetValue(contentTag, out List<ExtendedContent> foundContentTagsList))
                            foundContentTagsList.Add(extendedContent);
                        else
                            foundContentTagsDict.Add(contentTag, new List<ExtendedContent>() { extendedContent });
                    }

                string debugString = string.Empty;
                if (foundContentTagsDict.Count > 0)
                {
                    debugString = extendedMod.ModName + " Had The Following Content Tags, " + "\n";

                    foreach (KeyValuePair<ContentTag, List<ExtendedContent>> contentTagPair in foundContentTagsDict)
                    {
                        debugString += "\n" + "Tag: " + contentTagPair.Key.contentTagName + " | Associated Contents: ";
                        int counter = 0;
                        foreach (ExtendedContent extendedContent in contentTagPair.Value)
                        {
                            debugString += extendedContent.name;
                            if (counter != contentTagPair.Value.Count - 1)
                                debugString += ", ";
                            counter++;
                        }
                    }
                }
                else
                    debugString = extendedMod.ModName + " Had No Content Tags.";

                Log(debugString + "\n", DebugType.Developer);
            }
        }

        public static void LogDebugInstructionsFrom(CodeMatcher matcher)
        {
            var methodName = new StackTrace().GetFrame(1).GetMethod().Name;

            var instructionFormatter = new CodeInstructionFormatter(matcher.Length);
            var builder = new StringBuilder($"'{methodName}' Matcher Instructions:\n")
                .AppendLine(
                    String.Join(
                        "\n",
                        matcher
                            .InstructionEnumeration()
                            .Select(instructionFormatter.Format)
                    )
                )
                .AppendLine("End of matcher instructions.");

            Log(builder.ToString(), DebugType.Developer);
        }

        class CodeInstructionFormatter
        {
            public CodeInstructionFormatter(int instructionCount) {
                _instructionIndexPadLength = instructionCount.ToString().Length;
            }

            private int _instructionIndexPadLength;

            public string Format(CodeInstruction instruction, int index)
                => $"    IL_{index.ToString().PadLeft(_instructionIndexPadLength, '0')}: {instruction}";
        }
    }

    [System.Serializable]
    public class ExtendedLevelLogReport
    {
        public ExtendedLevel extendedLevel;

        public ExtendedLevelLogReport(ExtendedLevel newExtendedLevel)
        {
            extendedLevel = newExtendedLevel;
        }
    }

    [System.Serializable]
    public class ExtendedDungeonFlowLogReport
    {

    }
}
