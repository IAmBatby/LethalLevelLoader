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
                if (RoundManager.Instance != null && RoundManager.Instance.dungeonGenerator != null)
                    if (TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow flow))
                        returnFlow = flow;
                return (returnFlow);
            }
        }
        public static DungeonEvents GlobalDungeonEvents = new DungeonEvents();

        internal static void PatchVanillaDungeonLists()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                extendedDungeonFlow.DungeonID = RoundManager.Instance.dungeonFlowTypes.Length;
                RoundManager.Instance.dungeonFlowTypes = RoundManager.Instance.dungeonFlowTypes.AddItem(extendedDungeonFlow.dungeonFlow).ToArray();
                if (extendedDungeonFlow.dungeonFirstTimeAudio != null)
                    RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.AddItem(extendedDungeonFlow.dungeonFirstTimeAudio).ToArray();
            }
        }

        internal static void TryAddCurrentVanillaLevelDungeonFlow(DungeonGenerator dungeonGenerator, ExtendedLevel currentExtendedLevel)
        {
            if (dungeonGenerator.DungeonFlow != null && !RoundManager.Instance.dungeonFlowTypes.ToList().Contains(dungeonGenerator.DungeonFlow))
            {
                DebugHelper.Log("Level: " + currentExtendedLevel.selectableLevel.PlanetName + " Contains DungeonFlow: " + dungeonGenerator.DungeonFlow.name + " In DungeonGenerator That Was Not Found In RoundManager, Adding!");
                AssetBundleLoader.CreateVanillaExtendedDungeonFlow(dungeonGenerator.DungeonFlow);
                if (TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                {
                    IntWithRarity newIntWithRarity = new IntWithRarity();
                    newIntWithRarity.id = extendedDungeonFlow.DungeonID;
                    newIntWithRarity.rarity = 300;
                    currentExtendedLevel.selectableLevel.dungeonFlowTypes = currentExtendedLevel.selectableLevel.dungeonFlowTypes.AddItem(newIntWithRarity).ToArray();
                }
            }
        }

        internal static List<ExtendedDungeonFlowWithRarity> GetValidExtendedDungeonFlows(ExtendedLevel extendedLevel, bool debugResults)
        {
            string debugString = "Trying To Find All Matching DungeonFlows For Level: " + extendedLevel.NumberlessPlanetName + "\n";
            List<ExtendedDungeonFlowWithRarity> returnExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> potentialExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();

            //Add Vanilla DungeonFlows
            foreach (IntWithRarity specifiedDungeonFlowWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                if (TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[specifiedDungeonFlowWithRarity.id], out ExtendedDungeonFlow specifiedExtendedDungeonFlow))
                {
                    if (Settings.allDungeonFlowsRequireMatching == false)
                        returnExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(specifiedExtendedDungeonFlow, specifiedDungeonFlowWithRarity.rarity));
                    else
                        potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(specifiedExtendedDungeonFlow, specifiedDungeonFlowWithRarity.rarity));
                }

            //Add Custom DungeonFlows
            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, 0));

            debugString += "Potential DungeonFlows Collected, List Below: " + "\n";
            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList.Concat(potentialExtendedDungeonFlowsList))
                debugString += debugDungeon.extendedDungeonFlow.DungeonName + " - " + debugDungeon.rarity + ", ";

            debugString += "\n" + "\n" + "SelectableLevel - " + extendedLevel.selectableLevel.PlanetName + " Level Tags: ";
            foreach (ContentTag tag in extendedLevel.ContentTags)
                debugString += tag.contentTagName + ", ";
            debugString += "\n";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList)
                debugString += "\n" + "DungeonFlow " + (returnExtendedDungeonFlowsList.IndexOf(debugDungeon) + 1) + ". : " + debugDungeon.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With A Rarity Of: " + debugDungeon.rarity + " Based On The Levels DungeonFlowTypes!" + "\n";
            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                customDungeonFlow.rarity = customDungeonFlow.extendedDungeonFlow.levelMatchingProperties.GetDynamicRarity(extendedLevel);
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

            return (returnExtendedDungeonFlowsList);
        }

        internal static void RefreshDungeonFlowIDs()
        {
            DebugHelper.Log("Re-Adjusting DungeonFlowTypes Array For Late Arriving Vanilla DungeonFlow");
            List<DungeonFlow> cachedDungeonFlowTypes = new List<DungeonFlow>();
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
            RoundManager.Instance.dungeonFlowTypes = cachedDungeonFlowTypes.ToArray();
        }

        internal static int GetHighestRarityViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors)
        {
            int returnInt = 0;
            foreach (Vector2WithRarity vectorWithRarity in matchingVectors)
                if (vectorWithRarity.Rarity >= returnInt)
                    if ((comparingValue >= vectorWithRarity.Min) && (comparingValue <= vectorWithRarity.Max))
                        returnInt = vectorWithRarity.Rarity;
            return (returnInt);
        }

        internal static int GetHighestRarityViaMatchingNormalizedString(string comparingString, List<StringWithRarity> matchingStrings)
        {
            return (GetHighestRarityViaMatchingNormalizedStrings(new List<string>() { comparingString }, matchingStrings));
        }

        internal static int GetHighestRarityViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings)
        {
            int returnInt = 0;
            foreach (StringWithRarity stringWithRarity in matchingStrings)
                foreach (string comparingString in new List<string>(comparingStrings))
                    if (stringWithRarity.Rarity >= returnInt)
                        if (stringWithRarity.Name.Sanitized().Contains(comparingString.Sanitized()) || comparingString.Sanitized().Contains(stringWithRarity.Name.Sanitized()))
                            returnInt = stringWithRarity.Rarity;
            return (returnInt);
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
    }
}