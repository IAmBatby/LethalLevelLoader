using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public static class ExtendedDungeonFlowExtensions
    {
        public static ExtendedDungeonFlow GetExtendedDungeonFlow(this DungeonFlow flow) => ExtendedContentManager<ExtendedDungeonFlow, DungeonFlow>.ExtensionDictionary[flow];
    }
}
