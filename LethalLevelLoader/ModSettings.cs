using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public enum PreviewInfoType { Price, Difficulty, Weather, History, All, None, Vanilla, Override };
    public enum SortInfoType { Price, Difficulty, Tag, LastTraveled, None }
    public enum FilterInfoType { Price, Weather, Tag, TraveledThisQuota, TraveledThisRun, None}

    public static class ModSettings
    {
        public static PreviewInfoType levelPreviewInfoType = PreviewInfoType.Weather;
        public static SortInfoType levelPreviewSortType = SortInfoType.None;
        public static FilterInfoType levelPreviewFilterType = FilterInfoType.None;

        public static string GetOverridePreviewInfo(ExtendedLevel extendedLevel)
        {
            string returnString = string.Empty;

            return (returnString);
        }
    }
}
