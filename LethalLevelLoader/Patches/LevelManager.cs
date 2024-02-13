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

        public static List<DayHistory> dayHistoryList = new List<DayHistory>();
        public static int daysTotal;
        public static int quotasTotal;

        public static int invalidSaveLevelID = -1;

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
            foreach (ExtendedLevel level in new List<ExtendedLevel>(PatchedContent.CustomExtendedLevels))
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
                            extendedLevel.routeNode = compatibleRouteNoun.result;
                            extendedLevel.routeConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                            extendedLevel.RoutePrice = extendedLevel.routeNode.itemCost;
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

        public static void LogDayHistory()
        {
            DayHistory newDayHistory = new DayHistory();
            daysTotal++;

            newDayHistory.extendedLevel = GetExtendedLevel(StartOfRound.Instance.currentLevel);
            DungeonManager.TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow);
            newDayHistory.extendedDungeonFlow = extendedDungeonFlow;
            newDayHistory.day = daysTotal;
            newDayHistory.quota = TimeOfDay.Instance.timesFulfilledQuota;
            newDayHistory.weatherEffect = StartOfRound.Instance.currentLevel.currentWeather;

            DebugHelper.Log("Created New Day History Log! PlanetName: " + newDayHistory.extendedLevel.NumberlessPlanetName + " , DungeonName: " + newDayHistory.extendedDungeonFlow.dungeonDisplayName + " , Quota: " + newDayHistory.quota + " , Day: " + newDayHistory.day + " , Weather: " + newDayHistory.weatherEffect.ToString());

            if (dayHistoryList == null)
                dayHistoryList = new List<DayHistory>();

            dayHistoryList.Add(newDayHistory);
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