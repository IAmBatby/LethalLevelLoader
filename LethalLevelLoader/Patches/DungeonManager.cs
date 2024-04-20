using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LethalLevelLoader
{
    public class DungeonManager
    {
        public static ExtendedDungeonFlow CurrentExtendedDungeonFlow
        {
            get
            {
                ExtendedDungeonFlow returnFlow = null;
                if (Patches.RoundManager != null && Patches.RoundManager.dungeonGenerator != null)
                    if (TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow flow))
                        returnFlow = flow;
                return (returnFlow);
            }
        }
        public static DungeonEvents GlobalDungeonEvents = new DungeonEvents();

        internal static void PatchVanillaDungeonLists()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                extendedDungeonFlow.DungeonID = Patches.RoundManager.dungeonFlowTypes.Length;
                IndoorMapType newIndoorMapType = new IndoorMapType();
                newIndoorMapType.dungeonFlow = extendedDungeonFlow.dungeonFlow;
                newIndoorMapType.MapTileSize = extendedDungeonFlow.MapTileSize;
                Patches.RoundManager.dungeonFlowTypes = Patches.RoundManager.dungeonFlowTypes.AddItem(newIndoorMapType).ToArray();
                if (extendedDungeonFlow.dungeonFirstTimeAudio != null)
                    Patches.RoundManager.firstTimeDungeonAudios = Patches.RoundManager.firstTimeDungeonAudios.AddItem(extendedDungeonFlow.dungeonFirstTimeAudio).ToArray();
            }
        }

        //Obsolete
        internal static void TryAddCurrentVanillaLevelDungeonFlow(DungeonGenerator dungeonGenerator, ExtendedLevel currentExtendedLevel)
        {
            if (dungeonGenerator.DungeonFlow != null && !Patches.RoundManager.GetDungeonFlows().Contains(dungeonGenerator.DungeonFlow))
            {
                DebugHelper.Log("Level: " + currentExtendedLevel.SelectableLevel.PlanetName + " Contains DungeonFlow: " + dungeonGenerator.DungeonFlow.name + " In DungeonGenerator That Was Not Found In RoundManager, Adding!");
                AssetBundleLoader.CreateVanillaExtendedDungeonFlow(dungeonGenerator.DungeonFlow);
                if (TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                {
                    IntWithRarity newIntWithRarity = new IntWithRarity();
                    newIntWithRarity.id = extendedDungeonFlow.DungeonID;
                    newIntWithRarity.rarity = 300;
                    currentExtendedLevel.SelectableLevel.dungeonFlowTypes = currentExtendedLevel.SelectableLevel.dungeonFlowTypes.AddItem(newIntWithRarity).ToArray();
                }
            }
        }

        internal static List<ExtendedDungeonFlowWithRarity> GetValidExtendedDungeonFlows(ExtendedLevel extendedLevel, bool debugResults)
        {
            DebugStopwatch.StartStopWatch("Get Valid ExtendedDungeonFlows");
            string debugString = "Trying To Find All Matching DungeonFlows For Level: " + extendedLevel.NumberlessPlanetName + "\n";
            List<ExtendedDungeonFlowWithRarity> returnExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> potentialExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();

            //Add Vanilla DungeonFlows
            /*foreach (IntWithRarity specifiedDungeonFlowWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                if (TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonFlowTypes[specifiedDungeonFlowWithRarity.id], out ExtendedDungeonFlow specifiedExtendedDungeonFlow))
                {
                    if (Settings.allDungeonFlowsRequireMatching == false)
                        returnExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(specifiedExtendedDungeonFlow, specifiedDungeonFlowWithRarity.rarity));
                    else
                        potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(specifiedExtendedDungeonFlow, specifiedDungeonFlowWithRarity.rarity));
                }*/

            foreach (ExtendedDungeonFlow vanillaDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(vanillaDungeonFlow, 0));

            //Add Custom DungeonFlows
            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, 0));

            debugString += "Potential DungeonFlows Collected, List Below: " + "\n";
            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList.Concat(potentialExtendedDungeonFlowsList))
                debugString += debugDungeon.extendedDungeonFlow.DungeonName + " - " + debugDungeon.rarity + ", ";

            debugString += "\n" + "\n" + "SelectableLevel - " + extendedLevel.SelectableLevel.PlanetName + " Level Tags: ";
            foreach (ContentTag tag in extendedLevel.ContentTags)
                debugString += tag.contentTagName + ", ";
            debugString += "\n";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList)
                debugString += "\n" + "DungeonFlow " + (returnExtendedDungeonFlowsList.IndexOf(debugDungeon) + 1) + ". : " + debugDungeon.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With A Rarity Of: " + debugDungeon.rarity + " Based On The Levels DungeonFlowTypes!" + "\n";
            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                customDungeonFlow.rarity = customDungeonFlow.extendedDungeonFlow.LevelMatchingProperties.GetDynamicRarity(extendedLevel);
                if (customDungeonFlow.rarity != 0)
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
            }

            if (debugResults == true)
            {
                debugString += "\n" + "Matching DungeonFlows Collected, Count Is: " + returnExtendedDungeonFlowsList.Count + "\n" + "\n";
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in returnExtendedDungeonFlowsList)
                    debugString += "\n" + extendedDungeonFlowWithRarity.extendedDungeonFlow.DungeonName + " - (" + extendedDungeonFlowWithRarity.rarity + ")";
                DebugHelper.Log(debugString);
            }

            DebugStopwatch.StopStopWatch("Get Valid ExtendedDungeonFlows");

            return (returnExtendedDungeonFlowsList);
        }

        internal static void RefreshDungeonFlowIDs()
        {
            DebugHelper.Log("Re-Adjusting DungeonFlowTypes Array For Late Arriving Vanilla DungeonFlow");
            
            List<DungeonFlow> cachedDungeonFlowTypes = new List<DungeonFlow>();
            List<IndoorMapType> indoorMapTypes = new List<IndoorMapType>();
            foreach (ExtendedDungeonFlow vanillaDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
            {
                vanillaDungeonFlow.DungeonID = cachedDungeonFlowTypes.Count;
                cachedDungeonFlowTypes.Add(vanillaDungeonFlow.dungeonFlow);
            }
            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                customDungeonFlow.DungeonID = cachedDungeonFlowTypes.Count;
                cachedDungeonFlowTypes.Add(customDungeonFlow.dungeonFlow);
            }

            foreach (DungeonFlow dungeonFlow in cachedDungeonFlowTypes)
            {
                IndoorMapType newIndoorMapType = new IndoorMapType();
                newIndoorMapType.dungeonFlow = dungeonFlow;
                newIndoorMapType.MapTileSize = 1f;
                indoorMapTypes.Add(newIndoorMapType);
            }
            Patches.RoundManager.dungeonFlowTypes = indoorMapTypes.ToArray();
        }

        internal static bool TryGetExtendedDungeonFlow(DungeonFlow dungeonFlow, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType contentType = ContentType.Any)
        {
            returnExtendedDungeonFlow = null;
            List<ExtendedDungeonFlow> extendedDungeonFlowsList = null;

            if (dungeonFlow == null) return (false);

            if (contentType == ContentType.Any)
                extendedDungeonFlowsList = PatchedContent.ExtendedDungeonFlows;
            else if (contentType == ContentType.Custom)
                extendedDungeonFlowsList = PatchedContent.CustomExtendedDungeonFlows;
            else if (contentType == ContentType.Vanilla)
                extendedDungeonFlowsList = PatchedContent.VanillaExtendedDungeonFlows;

            foreach (ExtendedDungeonFlow extendedDungeonFlow in extendedDungeonFlowsList)
                if (extendedDungeonFlow.dungeonFlow == dungeonFlow)
                    returnExtendedDungeonFlow = extendedDungeonFlow;

            return (returnExtendedDungeonFlow != null);
        }

        internal static bool TryGetExtendedDungeonFlow(IndoorMapType indoorMapType, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType contentType = ContentType.Any)
        {
            return (TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out returnExtendedDungeonFlow, contentType));
        }
    }
}