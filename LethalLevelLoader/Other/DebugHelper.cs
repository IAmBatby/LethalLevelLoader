using DunGen.Graph;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

namespace LethalLevelLoader
{
    public static class DebugHelper
    {
        public static string logAuthor = "Batby";

        public static void Log(string log)
        {
            string logString = "LethalLib (" + logAuthor + "): ";
            logString += log;
            Debug.Log(logString);
        }

        public static void DebugTerminalKeyword(TerminalKeyword terminalKeyword)
        {
            string logString = "Info For " + terminalKeyword.word + ") TerminalKeyword!" + "\n" + "\n";
            logString += "Word: " + terminalKeyword.word + "\n";
            logString += "isVerb?: " + terminalKeyword.isVerb + "\n";
            logString += "CompatibleNouns :" + "\n";

            foreach (CompatibleNoun compatibleNoun in terminalKeyword.compatibleNouns)
                logString += compatibleNoun.noun + " | " + compatibleNoun.result + "\n";

            logString += "SpecialKeywordResult: " + terminalKeyword.specialKeywordResult + "\n";
            logString += "AccessTerminalObjects?: " + terminalKeyword.accessTerminalObjects + "\n";
            logString += "DefaultVerb: " + terminalKeyword.defaultVerb.name + "\n";

            Log(logString + "\n" + "\n");
        }

        public static void DebugTerminalNode(TerminalNode terminalNode)
        {
            string logString = "Info For " + terminalNode.name + ") TerminalNode!" + "\n" + "\n";
            logString += "Display Text: " + terminalNode.displayText + "\n";
            logString += "Accept Anything?: " + terminalNode.acceptAnything + "\n";
            logString += "Override Options?: " + terminalNode.overrideOptions + "\n";
            logString += "Display Planet Info (LevelID): " + terminalNode.displayPlanetInfo + "\n";
            logString += "Buy Reroute To Moon (LevelID): " + terminalNode.buyRerouteToMoon + "\n";
            logString += "Is Confirmation Node?: " + terminalNode.isConfirmationNode + "\n";
            logString += "Terminal Options (CompatibleNouns) :" + "\n";

            foreach (CompatibleNoun compatibleNoun in terminalNode.terminalOptions)
                logString += compatibleNoun.noun + " | " + compatibleNoun.result + "\n";

            Log(logString + "\n" + "\n");
        }

        public static void DebugInjectedLevels()
        {
            string logString = "Injected Levels List: " + "\n" + "\n";

            int counter = 0;
            if (StartOfRound.Instance != null)
            {
                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {
                    logString += counter + ". " + level.PlanetName + " (" + level.levelID + ") " + "\n";
                    counter++;
                }

                logString += "Current Level Is: " + StartOfRound.Instance.currentLevel.PlanetName + " (" + StartOfRound.Instance.currentLevel.levelID + ") " + "\n";
            }

            Log(logString + "\n" + "\n");
        }



        public static void DebugAllLevels()
        {
            string logString = "All Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.allLevelsList)
                logString += extendedLevel.selectableLevel.PlanetName + " (" + extendedLevel.selectableLevel.levelID + ") " + "\n";

            Log(logString + "\n");
        }

        public static void DebugVanillaLevels()
        {
            string logString = "Vanilla Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.vanillaLevelsList)
                logString += extendedLevel.selectableLevel.PlanetName + " (" + extendedLevel.selectableLevel.levelID + ") " + "\n";

            Log(logString + "\n");
        }

        public static void DebugCustomLevels()
        {
            string logString = "Custom Levels List: " + "\n" + "\n";

            foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.customLevelsList)
                logString += extendedLevel.selectableLevel.PlanetName + " (" + extendedLevel.selectableLevel.levelID + ") " + "\n";

            Log(logString + "\n");
        }

        public static void DebugScrapedVanillaContent()
        {
            Log("Obtained (" + ContentExtractor.vanillaItemsList.Count + " / 68) Vanilla Item References");

            Log("Obtained (" + ContentExtractor.vanillaEnemiesList.Count + " / 20) Vanilla Enemy References");

            Log("Obtained (" + ContentExtractor.vanillaSpawnableOutsideMapObjectsList.Count + " / 11) Vanilla Outside Object References");

            Log("Obtained (" + ContentExtractor.vanillaSpawnableInsideMapObjectsList.Count + " / 2) Vanilla Inside Object References");


            Log("Obtained (" + ContentExtractor.vanillaAmbienceLibrariesList.Count + " / 3) Vanilla Ambience Library References");

            Log("Obtained (" + ContentExtractor.vanillaAudioMixerGroupsList.Count + " / 00) Vanilla Audio Mixing Group References");

            foreach (AudioMixerGroup audioMix in ContentExtractor.vanillaAudioMixerGroupsList)
                Log("AudioMixerGroup Name: " + audioMix.name);
        }

        public static void DebugAudioMixerGroups()
        {

        }


        public static void DebugSelectableLevelReferences(ExtendedLevel extendedLevel)
        {
            string logString = "Logging SelectableLevel References For Moon: " + extendedLevel.NumberlessPlanetName + " (" + extendedLevel.levelType.ToString() + ")." + "\n";

            logString += "Inside Enemies" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.selectableLevel.Enemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";

            logString += "Outside Enemies (Nighttime)" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.selectableLevel.OutsideEnemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";

            logString += "Outside Enemies (daytime)" + "\n" + "\n";

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.selectableLevel.DaytimeEnemies)
                logString += "Enemy Type: " + spawnableEnemy.enemyType.enemyName + " , Rarity: " + spawnableEnemy.rarity + " , Prefab Status: " + (spawnableEnemy.enemyType.enemyPrefab != null) + "\n";


            Log(logString + "\n");
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

            foreach (ExtendedDungeonFlow dungeonFlow in DungeonFlow_Patch.allExtendedDungeonsList)
                debugString += dungeonFlow.dungeonFlow.name;

            Log(debugString);

            debugString = "Vanilla ExtendedDungeons: " + "\n" + "\n";

            foreach (ExtendedDungeonFlow dungeonFlow in DungeonFlow_Patch.vanillaDungeonFlowsList)
                debugString += dungeonFlow.dungeonFlow.name;

            Log(debugString);

            debugString = "Custom ExtendedDungeons: " + "\n" + "\n";

            foreach (ExtendedDungeonFlow dungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                debugString += dungeonFlow.dungeonFlow.name;

            Log(debugString);
        }

        public static void DebugPlanetWeatherRandomisation(int players, List<SelectableLevel> selectableLevelsList)
        {
            StartOfRound startOfRound = StartOfRound.Instance;

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

            DebugHelper.Log(debugString);
        }
    }

}
