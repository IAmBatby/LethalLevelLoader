using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public static class ExtendedLevelExtensions
    {
        public static ExtendedLevel GetExtendedLevel(this SelectableLevel level)
        {
            return (LevelManager.ExtensionDictionary[level]);
        }
    }
}
