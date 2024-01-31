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
    public class SelectableLevel_Patch
    {
        public static List<DayHistory> dayHistoryList = new List<DayHistory>();
        public static int daysTotal;
        public static int quotasTotal;

        internal static void RestoreCustomContent()
        {
            foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);
        }

        internal static void PatchVanillaLevelLists()
        {
            StartOfRound.Instance.levels = PatchedContent.SeletectableLevels.ToArray();
            Terminal_Patch.Terminal.moonsCatalogueList = PatchedContent.MoonsCatalogue.ToArray();
        }

        public static bool TryGetExtendedLevel(SelectableLevel selectableLevel, out ExtendedLevel returnExtendedLevel, ContentType levelType = ContentType.Any)
        {
            returnExtendedLevel = null;
            List<ExtendedLevel> extendedLevelsList = new List<ExtendedLevel>();

            switch (levelType)
            {
                case ContentType.Vanilla:
                    extendedLevelsList = PatchedContent.VanillaExtendedLevels;
                    break;
                case ContentType.Custom:
                    extendedLevelsList = PatchedContent.CustomExtendedLevels;
                    break;
                case ContentType.Any:
                    extendedLevelsList = PatchedContent.ExtendedLevels;
                    break;
            }

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
            DungeonFlow_Patch.TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow);
            newDayHistory.extendedDungeonFlow = extendedDungeonFlow;
            newDayHistory.day = daysTotal;
            newDayHistory.quota = TimeOfDay.Instance.timesFulfilledQuota;
            newDayHistory.weatherEffect = StartOfRound.Instance.currentLevel.currentWeather;

            DebugHelper.Log("Created New Day History Log! PlanetName: " + newDayHistory.extendedLevel.NumberlessPlanetName + " , DungeonName: " + newDayHistory.extendedDungeonFlow.dungeonDisplayName + " , Quota: " + newDayHistory.quota + " , Day: " + newDayHistory.day + " , Weather: " + newDayHistory.weatherEffect.ToString());

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