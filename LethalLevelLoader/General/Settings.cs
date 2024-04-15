using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public enum PreviewInfoType { Price, Difficulty, Weather, History, All, None, Vanilla, Override };
    public enum SortInfoType { Price, Difficulty, Tag, LastTraveled, None }
    public enum FilterInfoType { Price, Weather, Tag, TraveledThisQuota, TraveledThisRun, None}
    public enum SimulateInfoType { Percentage, Rarity }
    public enum DebugType { User, Developer, IAmBatby, All }

    public static class Settings
    {
        public static PreviewInfoType levelPreviewInfoType = PreviewInfoType.Weather;
        public static SortInfoType levelPreviewSortType = SortInfoType.None;
        public static FilterInfoType levelPreviewFilterType = FilterInfoType.None;
        public static SimulateInfoType levelSimulateInfoType = SimulateInfoType.Percentage;
        public static DebugType debugType = DebugType.All;
        public static bool allDungeonFlowsRequireMatching = false;
        public static int moonsCatalogueSplitCount = 3;

        public static string GetOverridePreviewInfo(ExtendedLevel extendedLevel)
        {
            string returnString = string.Empty;

            return (returnString);
        }
    }
}
