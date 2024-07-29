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
                if (Patches.StartOfRound != null)
                    if (TryGetExtendedLevel(Patches.StartOfRound.currentLevel, out ExtendedLevel level))
                        returnLevel = level;
                return returnLevel;
            }
        }
        public static LevelEvents GlobalLevelEvents = new LevelEvents();

        public static List<DayHistory> dayHistoryList = new List<DayHistory>();
        public static int daysTotal;
        public static int quotasTotal;

        public static int invalidSaveLevelID = -1;

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

        internal static void PatchVanillaLevelLists()
        {
            Patches.StartOfRound.levels = PatchedContent.SelectableLevels.ToArray();
            TerminalManager.Terminal.moonsCatalogueList = PatchedContent.MoonsCatalogue.ToArray();
        }

        internal static void InitializeShipAnimatorOverrideController()
        {
            Animator shipAnimator = Patches.StartOfRound.shipAnimator;

            List<GameObject> childObjects = new List<GameObject>();
            foreach (Transform child in shipAnimator.GetComponentsInChildren<Transform>(includeInactive: true))
                if (!childObjects.Contains(child.gameObject))
                    childObjects.Add(child.gameObject);

            AnimatorOverrideController overrideController = new AnimatorOverrideController(shipAnimator.runtimeAnimatorController);
            LevelLoader.shipAnimatorOverrideController = overrideController;
            LevelLoader.defaultShipFlyToMoonClip = overrideController["HangarShipLandB"];
            LevelLoader.defaultShipFlyFromMoonClip = overrideController["ShipLeave"];

            foreach (AnimationClip animationClip in shipAnimator.runtimeAnimatorController.animationClips)
                overrideController[animationClip.name] = animationClip;

            shipAnimator.runtimeAnimatorController = overrideController;
            //shipAnimator.Play("Base Layer.ShipIdle", layer: 0, normalizedTime: 1.0f);

            //shipAnimator.enabled = false;
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
                if (extendedLevel.SelectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel != null);
        }

        public static ExtendedLevel GetExtendedLevel(SelectableLevel selectableLevel)
        {
            ExtendedLevel returnExtendedLevel = null;

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                if (extendedLevel.SelectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel);
        }

        public static void RegisterExtendedFootstepSurfaces(ExtendedLevel extendedLevel)
        {
            /*List<FootstepSurface> currentFootstepSurfaces = Patches.StartOfRound.footstepSurfaces.ToList();

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
                                Patches.StartOfRound.footstepSurfaces = Patches.StartOfRound.footstepSurfaces.AddItem(extendedFootstepSurface.footstepSurface).ToArray();
                                extendedFootstepSurface.arrayIndex = Patches.StartOfRound.footstepSurfaces.Length - 1;
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
            foreach (FootstepSurface footstepSurface in Patches.StartOfRound.footstepSurfaces)
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
                DebugHelper.Log("Risk Level Of " + vanillaLevel.NumberlessPlanetName + " Is: " + vanillaLevel.SelectableLevel.riskLevel, DebugType.Developer);
                if (!vanillaLevel.SelectableLevel.riskLevel.Contains("Safe") && !string.IsNullOrEmpty(vanillaLevel.SelectableLevel.riskLevel))
                {
                    if (vanillaRiskLevelDictionary.TryGetValue(vanillaLevel.SelectableLevel.riskLevel, out List<int> dynamicDifficultyRatingList))
                        dynamicDifficultyRatingList.Add(vanillaLevel.CalculatedDifficultyRating);
                    else
                        vanillaRiskLevelDictionary.Add(vanillaLevel.SelectableLevel.riskLevel, new List<int>() { vanillaLevel.CalculatedDifficultyRating });
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
                DebugHelper.Log(debugString, DebugType.Developer);
            }

            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
                    foreach (KeyValuePair<string, List<int>> vanillaRiskLevel in vanillaRiskLevelDictionary)
                        if (dynamicRiskLevelPair.Key.Equals(vanillaRiskLevel.Key))
                        {
                            DebugHelper.Log("Setting RiskLevel " + vanillaRiskLevel.Key + " To " + (int)vanillaRiskLevel.Value.Average(), DebugType.Developer);
                            dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key] = Mathf.RoundToInt((float)vanillaRiskLevel.Value.Average());
                        }

            DebugHelper.Log("Starting To Assign - and + Risk Levels", DebugType.Developer);
            int counter = 0;
            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
            {
                string previousFullRiskLevel = string.Empty;
                string currentFullRiskLevel = string.Empty;
                string nextFullRiskLevel = string.Empty;
                DebugHelper.Log("Trying To Assign Value To Risk Level: " + dynamicRiskLevelPair.Key, DebugType.Developer);

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

                DebugHelper.Log("Risk Level: " + dynamicRiskLevelPair.Key + " Was Assigned Calculated Difficulty Of: " + dynamicRiskLevelDictionary[dynamicRiskLevelPair.Key].ToString(), DebugType.Developer);
                counter++;
            }

            foreach (KeyValuePair<string, int> dynamicRiskLevelPair in new Dictionary<string, int>(dynamicRiskLevelDictionary))
                DebugHelper.Log("Dynamic Risk Level Pair: " + dynamicRiskLevelPair.Key + " (" + dynamicRiskLevelPair.Value + ")", DebugType.Developer);
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
                DebugHelper.Log("Ordered Calculated Risk Level: (" + calculatedRiskLevel.Value + ") - " + calculatedRiskLevel.Key, DebugType.Developer);

            foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
            {
                if (customLevel.OverrideDynamicRiskLevelAssignment == false)
                {
                    int customLevelCalculatedDifficultyRating = customLevel.CalculatedDifficultyRating;
                    int closestCalculatedRiskLevelRating = orderedCalculatedDifficultyList[0];

                    closestCalculatedRiskLevelRating = orderedCalculatedDifficultyList.OrderBy(item => Math.Abs(customLevelCalculatedDifficultyRating - item)).First();

                    if (closestCalculatedRiskLevelRating != 0)
                        customLevel.SelectableLevel.riskLevel = assignmentRiskLevelDictionary[closestCalculatedRiskLevelRating];
                }
            }

            List<ExtendedLevel> extendedLevelsOrdered = new List<ExtendedLevel>(PatchedContent.ExtendedLevels).OrderBy(o => o.CalculatedDifficultyRating).ToList();

            foreach (ExtendedLevel extendedLevel in extendedLevelsOrdered)
                DebugHelper.Log(extendedLevel.NumberlessPlanetName + " (" + extendedLevel.SelectableLevel.riskLevel + ") " + " (" + extendedLevel.CalculatedDifficultyRating + ")", DebugType.Developer);
        }

        public static void LogDayHistory()
        {
            //Heavy early returns here because this runs from a DunGen patch and needs to be safe for unconventional Unity-Editor generation usage.
            if (Plugin.IsSetupComplete == false || Patches.StartOfRound == null || Patches.RoundManager == null || TimeOfDay.Instance == null)
            {
                DebugHelper.LogWarning("Game Seems Uninitialized, Exiting LogDayHistory Early!", DebugType.Developer);
                return;
            }

            DayHistory newDayHistory = new DayHistory();
            daysTotal++;

            newDayHistory.allViableOptions = DungeonManager.GetValidExtendedDungeonFlows(CurrentExtendedLevel, false).Select(e => e.extendedDungeonFlow).ToList();
            newDayHistory.extendedLevel = LevelManager.CurrentExtendedLevel;
            newDayHistory.extendedDungeonFlow = DungeonManager.CurrentExtendedDungeonFlow;
            newDayHistory.day = daysTotal;
            newDayHistory.quota = TimeOfDay.Instance.timesFulfilledQuota;
            newDayHistory.weatherEffect = Patches.StartOfRound.currentLevel.currentWeather;

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

            DebugHelper.Log(debugString, DebugType.User);

            if (dayHistoryList == null)
                dayHistoryList = new List<DayHistory>();

            dayHistoryList.Add(newDayHistory);
        }

        public static int CalculateExtendedLevelDifficultyRating(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            int returnRating = 0;
            string debugString = "Calculated Difficulty Rating For ExtendedLevel: " + extendedLevel.NumberlessPlanetName + "(" + extendedLevel.SelectableLevel.riskLevel + ")" + " ----- ";

            int baselineRouteValue = extendedLevel.RoutePrice;
            baselineRouteValue += extendedLevel.SelectableLevel.maxTotalScrapValue;
            returnRating += baselineRouteValue;
            debugString += "Baseline Route Value: " + baselineRouteValue + ", ";

            int scrapValue = 0;
            foreach (SpawnableItemWithRarity spawnableScrap in extendedLevel.SelectableLevel.spawnableScrap)
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

            int enemySpawnValue = (extendedLevel.SelectableLevel.maxEnemyPowerCount + extendedLevel.SelectableLevel.maxOutsideEnemyPowerCount + extendedLevel.SelectableLevel.maxDaytimeEnemyPowerCount) * 15;
            enemySpawnValue = enemySpawnValue * 2;
            returnRating += enemySpawnValue;
            debugString += "Enemy Spawn Value: " + enemySpawnValue + ", ";

            float enemyValue = 0;
            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.SelectableLevel.Enemies.Concat(extendedLevel.SelectableLevel.OutsideEnemies).Concat(extendedLevel.SelectableLevel.DaytimeEnemies))
            {
                if (spawnableEnemy.rarity != 0 && spawnableEnemy.enemyType != null)
                    if ((spawnableEnemy.rarity / 10) != 0)
                        enemyValue += (spawnableEnemy.enemyType.PowerLevel * 100) / (spawnableEnemy.rarity / 10);
            }
            returnRating += Mathf.RoundToInt(enemyValue);
            debugString += "Enemy Value: " + enemyValue + ", ";

            debugString += "Calculated Difficulty Value: " + returnRating + ", ";

            //returnRating = Mathf.RoundToInt(returnRating * Mathf.Lerp(1, extendedLevel.selectableLevel.factorySizeMultiplier, 0.25f));
            returnRating += Mathf.RoundToInt(returnRating * (extendedLevel.SelectableLevel.factorySizeMultiplier * 0.5f));

            debugString += "Factory Size Multiplier: " + extendedLevel.SelectableLevel.factorySizeMultiplier + ", ";

            debugString += "Multiplied Calculated Difficulty Value: " + returnRating;

            if (debugResults == true)
                DebugHelper.Log(debugString, DebugType.Developer);
            return (returnRating);
        }
    }

    public class DayHistory
    {
        public int quota;
        public int day;
        public ExtendedLevel extendedLevel;
        public List<ExtendedDungeonFlow> allViableOptions;
        public ExtendedDungeonFlow extendedDungeonFlow;
        public LevelWeatherType weatherEffect;
    }
}