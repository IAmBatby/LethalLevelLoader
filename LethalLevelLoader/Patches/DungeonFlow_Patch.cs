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
    public class DungeonFlow_Patch
    {
        internal static void AddExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.Log("Adding Dungeon Flow: " + extendedDungeonFlow.dungeonFlow.name);
            PatchedContent.ExtendedDungeonFlows.Add(extendedDungeonFlow);
            if (extendedDungeonFlow.dungeonType == ContentType.Custom)
            {
                extendedDungeonFlow.dungeonID = RoundManager.Instance.dungeonFlowTypes.Length;
                RoundManager.Instance.dungeonFlowTypes = RoundManager.Instance.dungeonFlowTypes.AddItem(extendedDungeonFlow.dungeonFlow).ToArray();
                if (extendedDungeonFlow.dungeonFirstTimeAudio != null)
                    RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.AddItem(extendedDungeonFlow.dungeonFirstTimeAudio).ToArray(); 
            }
        }

        internal static void TryAddCurrentVanillaLevelDungeonFlow(DungeonGenerator dungeonGenerator, ExtendedLevel currentExtendedLevel)
        {
            if (!OriginalContent.DungeonFlows.Contains(dungeonGenerator.DungeonFlow))
            {
                DebugHelper.Log("Level: " + currentExtendedLevel.selectableLevel.PlanetName + " Contains DungeonFlow: " + dungeonGenerator.DungeonFlow.name + " In DungeonGenerator That Was Not Found In RoundManager, Adding!");
                AssetBundleLoader.CreateVanillaExtendedDungeonFlow(dungeonGenerator.DungeonFlow);
                if (TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                {
                    IntWithRarity newIntWithRarity = new IntWithRarity();
                    newIntWithRarity.id = extendedDungeonFlow.dungeonID;
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

            if (extendedLevel.allowedDungeonContentTypes == ContentType.Vanilla || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
                foreach (IntWithRarity specifiedDungeonFlowWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                    if (TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[specifiedDungeonFlowWithRarity.id], out ExtendedDungeonFlow specifiedExtendedDungeonFlow))
                    {
                        returnExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(specifiedExtendedDungeonFlow, specifiedDungeonFlowWithRarity.rarity));
                    }

            if (extendedLevel.allowedDungeonContentTypes == ContentType.Custom || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
                foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                    potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, 0));

            debugString += "Potential DungeonFlows Collected, List Below: " + "\n";
            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList.Concat(potentialExtendedDungeonFlowsList))
                debugString += debugDungeon.extendedDungeonFlow.dungeonDisplayName + " - " + debugDungeon.rarity + ", ";

            debugString += "\n" + "\n" + "SelectableLevel - " + extendedLevel.selectableLevel.PlanetName + " Level Tags: ";
            foreach (string tag in extendedLevel.levelTags)
                debugString += tag + ", ";
            debugString += "\n";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList)
                debugString += "\n" + "DungeonFlow " + (returnExtendedDungeonFlowsList.IndexOf(debugDungeon) + 1) + ". : " + debugDungeon.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + "Based On The Levels DungeonFlowTypes!" + "\n";

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                ExtendedDungeonFlow extendedDungeonFlow = customDungeonFlow.extendedDungeonFlow;
                customDungeonFlow.UpdateRarity(GetHighestRarityViaMatchingNormalizedString(extendedLevel.contentSourceName, extendedDungeonFlow.manualContentSourceNameReferenceList));
                customDungeonFlow.UpdateRarity(GetHighestRarityViaMatchingNormalizedString(extendedLevel.NumberlessPlanetName, extendedDungeonFlow.manualPlanetNameReferenceList));
                customDungeonFlow.UpdateRarity(GetHighestRarityViaMatchingWithinRanges(extendedLevel.RoutePrice, extendedDungeonFlow.dynamicRoutePricesList));
                customDungeonFlow.UpdateRarity(GetHighestRarityViaMatchingNormalizedStrings(extendedLevel.levelTags, extendedDungeonFlow.dynamicLevelTagsList));
                customDungeonFlow.UpdateRarity(GetHighestRarityViaMatchingNormalizedString(extendedLevel.selectableLevel.currentWeather.ToString(), extendedDungeonFlow.dynamicCurrentWeatherList));

                if (customDungeonFlow.rarity != 0)
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
            }

            if (debugResults == true)
                DebugHelper.Log(debugString + "\n" + "Matching DungeonFlows Collected, Count Is: " + returnExtendedDungeonFlowsList.Count + "\n" + "\n");

            return (returnExtendedDungeonFlowsList);
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