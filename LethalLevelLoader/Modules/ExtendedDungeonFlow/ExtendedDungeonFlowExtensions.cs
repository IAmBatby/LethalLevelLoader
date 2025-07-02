using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public static class ExtendedDungeonFlowExtensions
    {
        public static ExtendedDungeonFlow AsExtended(this DungeonFlow flow) => DungeonManager.ExtensionDictionary[flow];
    }
}
