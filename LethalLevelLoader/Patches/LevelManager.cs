using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    public class LevelManager
    {
        public static ExtendedLevel CurrentExtendedLevel
        {
            get
            {
                ExtendedLevel returnLevel = null;
                if (StartOfRound.Instance != null)
                    if (TryGetExtendedLevel(StartOfRound.Instance.currentLevel, out ExtendedLevel level))
                        returnLevel = level;
                return returnLevel;
            }
        }
        public static LevelEvents GlobalLevelEvents = new LevelEvents();

        public static List<DayHistory> dayHistoryList = new List<DayHistory>();
        public static int daysTotal;
        public static int quotasTotal;

        public static int invalidSaveLevelID = -1;

        public static List<string> cachedFootstepSurfaceTagsList = new List<string>();
        public static List<Material> cachedExtendedFootstepSurfaceMaterialsList = new List<Material>();
        public static List<GameObject> cachedExtendedFootstepSurfaceGameObjectsList = new List<GameObject>(); 
        public static Dictionary<FootstepSurface, ExtendedFootstepSurface> cachedFootstepSurfacesDictionary = new Dictionary<FootstepSurface, ExtendedFootstepSurface>();

        public static Dictionary<string, int> dynamicRiskLevelDictionary = new Dictionary<string, int>()
        {
            {"D-", 0},
            {"D", 0},
            {"D+", 0},
            {"C-", 0},
            {"C", 0},
            {"C+", 0},
            {"B-", 0},
            {"B", 0},
            {"B+", 0},
            {"A-", 0},
            {"A", 0},
            {"A+", 0},
            {"S-", 0},
            {"S", 0},
            {"S+", 0},
            {"S++", 0 },
            {"S+++", 0}
        };

        internal static void ValidateLevelLists()
        {
            List<SelectableLevel> vanillaLevelsList = new List<SelectableLevel>(OriginalContent.SelectableLevels);
            List<SelectableLevel> vanillaMoonsCatalogueList = new List<SelectableLevel>(OriginalContent.MoonsCatalogue);
            List<SelectableLevel> startOfRoundLevelsList = new List<SelectableLevel>(StartOfRound.Instance.levels);

            foreach (SelectableLevel level in new List<SelectableLevel>(vanillaLevelsList))
                if (level.levelID > 8)
                    vanillaLevelsList.Remove(level);

            foreach (SelectableLevel level in new List<SelectableLevel>(vanillaMoonsCatalogueList))
                if (level.levelID > 8)
                    vanillaMoonsCatalogueList.Remove(level);

            foreach (SelectableLevel level in new List<SelectableLevel>(startOfRoundLevelsList))
                if (level.levelID > 8)
                    startOfRoundLevelsList.Remove(level);

            OriginalContent.SelectableLevels = vanillaLevelsList;
            OriginalContent.MoonsCatalogue = vanillaMoonsCatalogueList;

            PatchVanillaLevelLists();
        }

        internal static void PatchVanillaLevelLists()
        {
            StartOfRound.Instance.levels = PatchedContent.SeletectableLevels.ToArray();
            TerminalManager.Terminal.moonsCatalogueList = PatchedContent.MoonsCatalogue.ToArray();
        }

        internal static void RefreshCustomExtendedLevelIDs()
        {
            /*foreach (ExtendedLevel level in new List<ExtendedLevel>(PatchedContent.CustomExtendedLevels))
                if (level.isLethalExpansion == true)
                    level.SetLevelID();*/

            foreach (ExtendedLevel level in new List<ExtendedLevel>(PatchedContent.CustomExtendedLevels))
                if (level.isLethalExpansion == false)
                    level.SetLevelID();
        }

        internal static void RefreshLethalExpansionMoons()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.CustomExtendedLevels)
                if (extendedLevel.isLethalExpansion == true)
                {
                    foreach (CompatibleNoun compatibleRouteNoun in TerminalManager.routeKeyword.compatibleNouns)
                        if (compatibleRouteNoun.noun.name.ToLower().Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                        {
                            extendedLevel.RouteNode = compatibleRouteNoun.result;
                            extendedLevel.RouteConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                            extendedLevel.RoutePrice = extendedLevel.RouteNode.itemCost;
                            break;
                        }
                }

            RefreshCustomExtendedLevelIDs();
        }

        public static bool TryGetExtendedLevel(SelectableLevel selectableLevel, out ExtendedLevel returnExtendedLevel, ContentType levelType = ContentType.Any)
        {
            returnExtendedLevel = null;
            List<ExtendedLevel> extendedLevelsList = null;

            if (selectableLevel == null) return false;

            if (levelType == ContentType.Any)
                extendedLevelsList = PatchedContent.ExtendedLevels;
            else if (levelType == ContentType.Custom)
                extendedLevelsList = PatchedContent.CustomExtendedLevels;
            else if (levelType == ContentType.Vanilla)
                extendedLevelsList = PatchedContent.VanillaExtendedLevels;

            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel != null);
        }

        public static ExtendedLevel GetExtendedLevel(SelectableLevel selectableLevel)
        {
            ExtendedLevel returnExtendedLevel = null;

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel);
        }

        public static void RegisterExtendedFootstepSurfaces(ExtendedLevel extendedLevel)
        {
            /*List<FootstepSurface> currentFootstepSurfaces = StartOfRound.Instance.footstepSurfaces.ToList();

            if (extendedLevel.extendedFootstepSurfaces != null)
            {
                foreach (ExtendedFootstepSurface extendedFootstepSurface in extendedLevel.extendedFootstepSurfaces)
                {
                    if (extendedFootstepSurface != null && extendedFootstepSurface.footstepSurface != null)
                        if ((extendedFootstepSurface.associatedGameObjects != null && extendedFootstepSurface.associatedGameObjects.Count != 0) || (extendedFootstepSurface.associatedMaterials != null && extendedFootstepSurface.associatedMaterials.Count != 0))
                        {
                            if (!currentFootstepSurfaces.Contains(extendedFootstepSurface.footstepSurface))
                            {
                                DebugHelper.Log("Registering New Footstep Surface:  " + extendedFootstepSurface.footstepSurface.surfaceTag + " From ExtendedLevel: " + extendedLevel);
                                StartOfRound.Instance.footstepSurfaces = StartOfRound.Instance.footstepSurfaces.AddItem(extendedFootstepSurface.footstepSurface).ToArray();
                                extendedFootstepSurface.arrayIndex = StartOfRound.Instance.footstepSurfaces.Length - 1;
                            }
                        }
                }


                if (extendedLevel.extendedFootstepSurfaces.Count != 0)
                    RefreshCachedFootstepSurfaceData();
            }*/
        }

        public static void RefreshCachedFootstepSurfaceData()
        {
            /*cachedFootstepSurfacesDictionary = new Dictionary<FootstepSurface, ExtendedFootstepSurface>();
            foreach (FootstepSurface footstepSurface in StartOfRound.Instance.footstepSurfaces)
                cachedFootstepSurfacesDictionary.Add(footstepSurface, null);
            List<ExtendedFootstepSurface> extendedFootstepSurfaceList = new List<ExtendedFootstepSurface>();
            foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                foreach (ExtendedFootstepSurface extendedFootstepSurface in customLevel.extendedFootstepSurfaces)
                    if (!extendedFootstepSurfaceList.Contains(extendedFootstepSurface))
                    {
                        extendedFootstepSurfaceList.Add(extendedFootstepSurface);
                        if (cachedFootstepSurfacesDictionary.ContainsKey(extendedFootstepSurface.footstepSurface))
                            cachedFootstepSurfacesDictionary[extendedFootstepSurface.footstepSurface] = extendedFootstepSurface;
                    }
            cachedFootstepSurfaceTagsList = new List<string>();
            cachedExtendedFootstepSurfaceMaterialsList = new List<Material>();
            cachedExtendedFootstepSurfaceGameObjectsList = new List<GameObject>();
            foreach (FootstepSurface footstepSurface in cachedFootstepSurfacesDictionary.Keys)
            {
                if (footstepSurface != null)
                {
                    if (cachedFootstepSurfacesDictionary.TryGetValue(footstepSurface, out ExtendedFootstepSurface extendedFootstepSurface))
                    {
                        if (extendedFootstepSurface != null)
                        {
                            foreach (Material material in extendedFootstepSurface.associatedMaterials)
                                cachedExtendedFootstepSurfaceMaterialsList.Add(material);
                            foreach (GameObject gameObject in extendedFootstepSurface.associatedGameObjects)
                                cachedExtendedFootstepSurfaceGameObjectsList.Add(gameObject);
                        }
                    }
                    else
                        cachedFootstepSurfaceTagsList.Add(footstepSurface.surfaceTag);
                }
            }*/
        }

        public static void PopulateDynamicRiskLevelDictionary()
        {
            Dictionary<string, List<int>> vanillaRiskLevelDictionary = new Dictionary<string, List<int>>();

            foreach (ExtendedLevel vanillaLevel in PatchedContent.VanillaExtendedLevels)
            {
                DebugHelper.Log("Risk Level Of " + vanillaLevel.NumberlessPlanetName + " Is: " + vanillaLevel.selectableLevel.riskLevel);
                if (!vanillaLevel.selectableLevel.riskLevel.Contains("Safe") && !string.IsNullOrEmpty(vanillaLevel.selectableLevel.riskLevel))
                {
                    if (vanillaRiskLevelDictionary.TryGetValue(vanillaLevel.selectableLevel.riskLevel, out List<int> dynamicDifficultyRatingList))
                        dynamicDifficultyRatingList.Add(vanillaLevel.CalculatedDifficultyRating);
                    else
                        vanillaRiskLevelDictionary.Add(vanillaLevel.selectableLevel.riskLevel, new List<int>() { vanillaLevel.CalculatedDifficultyRating });
                }
            }

            foreach (KeyValuePair<string, List<int>> vanillaRiskLevel in vanillaRiskLevelDictionary)
            {
                string debugString = "Vanilla Risk Level Group (" + vanillaRiskLevel.Key + "): ";
                if (vanillaRiskLevel.Value != null)
                {
                    debugString += " Average - " + vanillaRiskLevel.Value.Average() + ", Values - ";
                    foreach (int calculatedDifficulty in vanillaRiskLevel.Value)
                        debugString += calculatedDifficulty.ToString() + ", ";
                }
                DebugHelper.Log(debugString);
            }

            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
                    foreach (KeyValuePair<string, List<int>> vanillaRiskLevel in vanillaRiskLevelDictionary)
                        if (dynamicRiskLevelPair.Key.Equals(vanillaRiskLevel.Key))
                        {
                            DebugHelper.Log("Setting RiskLevel " + vanillaRiskLevel.Key + " To " + (int)vanillaRiskLevel.Value.Average());
                            dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = Mathf.RoundToInt((float)vanillaRiskLevel.Value.Average());
                        }

            DebugHelper.Log("Starting To Assign - and + Risk Levels");
            int counter = 0;
            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
            {
                string previousFullRiskLevel = string.Empty;
                string currentFullRiskLevel = string.Empty;
                string nextFullRiskLevel = string.Empty;
                DebugHelper.Log("Trying To Assign Value To Risk Level: " + dynamicRiskLevelPair.Key);

                if (dynamicRiskLevelPair.Key.Contains("-"))
                {
                    if (counter != 0)
                        previousFullRiskLevel = dynamicRiskLevelDictionary.Keys.ToList()[counter - 2];
                    currentFullRiskLevel = dynamicRiskLevelDictionary.Keys.ToList()[counter + 1];

                    if (counter == 0)
                        dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = (dynamicRiskLevelDictionary[currentFullRiskLevel] / 2);
                    else
                        dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = Mathf.RoundToInt(Mathf.Lerp(dynamicRiskLevelDictionary[previousFullRiskLevel], dynamicRiskLevelDictionary[currentFullRiskLevel], 0.66f));

                }
                else if (dynamicRiskLevelPair.Key.Contains("+") && !dynamicRiskLevelPair.Key.Equals("S+"))
                {
                    currentFullRiskLevel = dynamicRiskLevelDictionary.Keys.ToList()[counter - 1];
                    if (!dynamicRiskLevelPair.Key.Contains("S"))
                        nextFullRiskLevel = dynamicRiskLevelDictionary.Keys.ToList()[counter + 2];

                    if (dynamicRiskLevelPair.Key.Equals("S++"))
                        dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = (dynamicRiskLevelDictionary[currentFullRiskLevel] * 2);
                    else if (dynamicRiskLevelPair.Key.Equals("S+++"))
                        dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = (dynamicRiskLevelDictionary[currentFullRiskLevel] * 3);
                    else
                        dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = Mathf.RoundToInt(Mathf.Lerp(dynamicRiskLevelDictionary[currentFullRiskLevel], dynamicRiskLevelDictionary[nextFullRiskLevel], 0.33f));
                }

                DebugHelper.Log("Risk Level: " + dynamicRiskLevelPair.Key + " Was Assigned Calculated Difficulty Of: " + dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key].ToString());
                counter++;
            }

            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
                DebugHelper.Log("Dynamic Risk Level Pair: " + dynamicRiskLevelPair.Key + " (" + dynamicRiskLevelPair.Value + ")");
        }

        public static void AssignCalculatedRiskLevels()
        {
            Dictionary<int, string> assignmentRiskLevelDictionary = new Dictionary<int, string>();
            List<int> orderedCalculatedDifficultyList = new List<int>(dynamicRiskLevelDictionary.Values);
            orderedCalculatedDifficultyList.Sort();

            foreach (int calculatedDifficultyValue in orderedCalculatedDifficultyList)
                foreach (KeyValuePair<string, int> calculatedRiskLevel in dynamicRiskLevelDictionary)
                    if (calculatedRiskLevel.Value == calculatedDifficultyValue)
                        assignmentRiskLevelDictionary.Add(calculatedDifficultyValue, calculatedRiskLevel.Key);

            foreach (KeyValuePair<int, string> calculatedRiskLevel in assignmentRiskLevelDictionary)
                DebugHelper.Log("Ordered Calculated Risk Level: (" + calculatedRiskLevel.Value + ") - " + calculatedRiskLevel.Key);

            foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
            {
                if (customLevel.overrideDynamicRiskLevelAssignment == false)
                {
                    int customLevelCalculatedDifficultyRating = customLevel.CalculatedDifficultyRating;
                    int closestCalculatedRiskLevelRating = orderedCalculatedDifficultyList[0];

                    closestCalculatedRiskLevelRating = orderedCalculatedDifficultyList.OrderBy(item => Math.Abs(customLevelCalculatedDifficultyRating - item)).First();

                    if (closestCalculatedRiskLevelRating != 0)
                        customLevel.selectableLevel.riskLevel = assignmentRiskLevelDictionary[closestCalculatedRiskLevelRating];
                }
            }

            List<ExtendedLevel> extendedLevelsOrdered = new List<ExtendedLevel>(PatchedContent.ExtendedLevels).OrderBy(o => o.CalculatedDifficultyRating).ToList();

            foreach (ExtendedLevel extendedLevel in extendedLevelsOrdered)
                DebugHelper.Log(extendedLevel.NumberlessPlanetName + " (" + extendedLevel.selectableLevel.riskLevel + ") " + " (" + extendedLevel.CalculatedDifficultyRating + ")");
        }

        public static void LogDayHistory()
        {
            //Heavy early returns here because this runs from a DunGen patch and needs to be safe for unconventional Unity-Editor generation usage.
            if (Plugin.IsSetupComplete == false || StartOfRound.Instance == null || RoundManager.Instance == null || TimeOfDay.Instance == null)
            {
                DebugHelper.LogWarning("Game Seems Uninitialized, Exiting LogDayHistory Early!");
                return;
            }

            DayHistory newDayHistory = new DayHistory();
            daysTotal++;

            newDayHistory.extendedLevel = LevelManager.CurrentExtendedLevel;
            newDayHistory.extendedDungeonFlow = DungeonManager.CurrentExtendedDungeonFlow;
            newDayHistory.day = daysTotal;
            newDayHistory.quota = TimeOfDay.Instance.timesFulfilledQuota;
            newDayHistory.weatherEffect = StartOfRound.Instance.currentLevel.currentWeather;

            string debugString = "Created New Day History Log! PlanetName: ";
            if (newDayHistory.extendedLevel != null)
                debugString += newDayHistory.extendedLevel.NumberlessPlanetName + " ,";
            else
                debugString += "MISSING EXTENDEDLEVEL ,";
            if (newDayHistory.extendedDungeonFlow != null)
                debugString += newDayHistory.extendedDungeonFlow.DungeonName + " ,";
            else
                debugString += "MISSING EXTENDEDDUNGEONFLOW ,";
            debugString += "Quota: " + newDayHistory.quota + " , Day: " + newDayHistory.day + " , Weather: " + newDayHistory.weatherEffect.ToString();

            DebugHelper.Log(debugString);

            if (dayHistoryList == null)
                dayHistoryList = new List<DayHistory>();

            dayHistoryList.Add(newDayHistory);
        }

        public static int CalculateExtendedLevelDifficultyRating(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            int returnRating = 0;
            string debugString = "Calculated Difficulty Rating For ExtendedLevel: " + extendedLevel.NumberlessPlanetName + "(" + extendedLevel.selectableLevel.riskLevel + ")" + " ----- ";

            int baselineRouteValue = extendedLevel.RoutePrice;
            baselineRouteValue += extendedLevel.selectableLevel.maxTotalScrapValue;
            returnRating += baselineRouteValue;
            debugString += "Baseline Route Value: " + baselineRouteValue + ", ";

            int scrapValue = 0;
            foreach (SpawnableItemWithRarity spawnableScrap in extendedLevel.selectableLevel.spawnableScrap)
            {
                if (spawnableScrap.spawnableItem != null)
                {
                    if (((spawnableScrap.spawnableItem.minValue + spawnableScrap.spawnableItem.maxValue) * 5) != 0 && spawnableScrap.rarity != 0)
                    {
                        if ((spawnableScrap.rarity / 10) != 0)
                            scrapValue += (spawnableScrap.spawnableItem.maxValue - spawnableScrap.spawnableItem.minValue) / (spawnableScrap.rarity / 10);
                    }
                }
            }
            returnRating += scrapValue;
            debugString += "Scrap Value: " + scrapValue + ", ";

            int enemySpawnValue = (extendedLevel.selectableLevel.maxEnemyPowerCount + extendedLevel.selectableLevel.maxOutsideEnemyPowerCount + extendedLevel.selectableLevel.maxDaytimeEnemyPowerCount) * 15;
            enemySpawnValue = enemySpawnValue * 2;
            returnRating += enemySpawnValue;
            debugString += "Enemy Spawn Value: " + enemySpawnValue + ", ";

            int enemyValue = 0;
            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.selectableLevel.Enemies.Concat(extendedLevel.selectableLevel.OutsideEnemies).Concat(extendedLevel.selectableLevel.DaytimeEnemies))
            {
                if (spawnableEnemy.rarity != 0 && spawnableEnemy.enemyType != null)
                    if ((spawnableEnemy.rarity / 10) != 0)
                        enemyValue += (spawnableEnemy.enemyType.PowerLevel * 100) / (spawnableEnemy.rarity / 10);
            }
            returnRating += enemyValue;
            debugString += "Enemy Value: " + enemyValue + ", ";

            debugString += "Calculated Difficulty Value: " + returnRating + ", ";

            //returnRating = Mathf.RoundToInt(returnRating * Mathf.Lerp(1, extendedLevel.selectableLevel.factorySizeMultiplier, 0.25f));
            returnRating += Mathf.RoundToInt(returnRating * (extendedLevel.selectableLevel.factorySizeMultiplier * 0.5f));

            debugString += "Factory Size Multiplier: " + extendedLevel.selectableLevel.factorySizeMultiplier + ", ";

            debugString += "Multiplied Calculated Difficulty Value: " + returnRating;

            if (debugResults == true)
                Debug.Log(debugString);
            return (returnRating);
        }
    }

    public class DayHistory
    {
        public int quota;
        public int day;
        public ExtendedLevel extendedLevel;
        public ExtendedDungeonFlow extendedDungeonFlow;
        public LevelWeatherType weatherEffect;
    }
}