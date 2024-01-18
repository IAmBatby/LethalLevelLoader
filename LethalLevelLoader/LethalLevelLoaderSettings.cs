using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public enum LevelPreviewInfoType { Weather, Price, Difficulty, Empty, Vanilla, Override };

    public static class LethalLevelLoaderSettings
    {
        public static LevelPreviewInfoType levelPreviewInfoType = LevelPreviewInfoType.Weather;

        public static string GetOverridePreviewInfo(ExtendedLevel extendedLevel)
        {
            string returnString = string.Empty;

            return (returnString);
        }
    }
}
