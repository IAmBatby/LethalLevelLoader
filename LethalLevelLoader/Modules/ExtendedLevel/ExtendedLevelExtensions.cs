using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public static class ExtendedLevelExtensions
    {
        public static ExtendedLevel AsExtended(this SelectableLevel level) => LevelManager.ExtensionDictionary[level];
    }
}
