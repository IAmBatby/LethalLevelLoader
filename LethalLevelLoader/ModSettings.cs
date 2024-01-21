using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public enum LevelPreviewInfoToggleType { Price, Difficulty, Weather, None, Vanilla, Override };
    public enum LevelPreviewInfoSortType { Price, Difficulty, Tag, LastTraveled, None }
    public enum LevelPreviewInfoFilterType { Price, Weather, Tag, Vanilla, Custom, TraveledThisQuota, TraveledThisRun, None}

    public static class ModSettings
    {
        public static LevelPreviewInfoToggleType levelPreviewInfoType = LevelPreviewInfoToggleType.Weather;
        public static LevelPreviewInfoSortType levelPreviewSortType = LevelPreviewInfoSortType.None;
        public static LevelPreviewInfoFilterType levelPreviewFilterType = LevelPreviewInfoFilterType.None;

        public static string GetOverridePreviewInfo(ExtendedLevel extendedLevel)
        {
            string returnString = string.Empty;

            return (returnString);
        }
    }
}
