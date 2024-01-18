using DunGen;
using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LethalLevelLoader
{
    public class DungeonFlow_Patch
    {
        public static List<ExtendedDungeonFlow> allExtendedDungeonsList = new List<ExtendedDungeonFlow>();
        public static List<ExtendedDungeonFlow> vanillaDungeonFlowsList = new List<ExtendedDungeonFlow>();
        public static List<ExtendedDungeonFlow> customDungeonFlowsList = new List<ExtendedDungeonFlow>();

        internal static void CreateExtendedDungeonFlow(DungeonFlow dungeon, int defaultRarity, string sourceName, AudioClip firstTimeDungeonAudio = null)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            newExtendedDungeonFlow.dungeonFlow = dungeon;
            newExtendedDungeonFlow.dungeonFirstTimeAudio = firstTimeDungeonAudio;
            newExtendedDungeonFlow.dungeonDefaultRarity = defaultRarity;

            AssetBundleLoader.obtainedExtendedDungeonFlowsList.Add(newExtendedDungeonFlow);
        }

        internal static void AddExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.Log("Adding Dungeon Flow: " + extendedDungeonFlow.dungeonFlow.name);
            if (extendedDungeonFlow.dungeonType == ContentType.Custom)
                customDungeonFlowsList.Add(extendedDungeonFlow);
            else
                vanillaDungeonFlowsList.Add(extendedDungeonFlow);

            allExtendedDungeonsList.Add(extendedDungeonFlow);
        }

        internal static ExtendedDungeonFlowWithRarity[] GetValidExtendedDungeonFlows(ExtendedLevel extendedLevel, string debugString)
        {
            RoundManager roundManager = RoundManager.Instance;
            debugString = "Trying To Find All Matching DungeonFlows For Level: " + extendedLevel.NumberlessPlanetName + "\n";

            List<ExtendedDungeonFlowWithRarity> potentialExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> returnExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> vanillaExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();

            DungeonFlow hardcodedLevelFlow = roundManager.dungeonGenerator.Generator.DungeonFlow;

            if (extendedLevel.allowedDungeonContentTypes == ContentType.Vanilla || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
            {

                //Hardcoded mess that creates and adds a dungeonflow thats directy in the dungeongenerator but not anywhere else
                //Currently the only usecase for this is March. Will refactor later.
                if (hardcodedLevelFlow != null)
                {
                    if (!TryGetExtendedDungeonFlow(hardcodedLevelFlow, out _))
                    {
                        debugString += "Level: " + extendedLevel.NumberlessPlanetName + " Contains DungeonFlow: " + hardcodedLevelFlow.name + " In DungeonGenerator That Was Not Found In RoundManager, Adding!" + "\n";
                        AssetBundleLoader.CreateVanillaExtendedDungeonFlow(hardcodedLevelFlow);
                    }

                    bool foundInSelectableLevel = false;
                    if (roundManager.dungeonFlowTypes.Length >= extendedLevel.selectableLevel.dungeonFlowTypes.Length)
                        foreach (IntWithRarity intWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                            if (roundManager.dungeonFlowTypes[intWithRarity.id] == hardcodedLevelFlow)
                                foundInSelectableLevel = true;

                    if (foundInSelectableLevel == false && TryGetExtendedDungeonFlow(hardcodedLevelFlow, out ExtendedDungeonFlow extendedHardcodedFlow))
                        vanillaExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(extendedHardcodedFlow, 300));
                }


                //Gets every Vanilla dungeon flow that's in the selectablelevel dungeonflowtypes list
                foreach (IntWithRarity intWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                    if (RoundManager.Instance.dungeonFlowTypes[intWithRarity.id] != null)
                        if (TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[intWithRarity.id], out ExtendedDungeonFlow outExtendedDungeonFlow, ContentType.Vanilla))
                            vanillaExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(outExtendedDungeonFlow, intWithRarity.rarity));
            }

            if (extendedLevel.allowedDungeonContentTypes == ContentType.Custom || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
            foreach (ExtendedDungeonFlow customDungeonFlow in customDungeonFlowsList)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, customDungeonFlow.dungeonDefaultRarity));


            //I use a buffer vanillaExtendedDungeonFlowsList here because we will do some user config stuff here later.
            foreach (ExtendedDungeonFlowWithRarity vanillaDungeonFlow in vanillaExtendedDungeonFlowsList)
            {
                /*DebugHelper.Log("PreConfig " + vanillaDungeonFlow.extendedDungeonFlow.name + " , " + vanillaDungeonFlow.rarity);
                if (potentialExtendedDungeonFlowsList.Count > 0)
                    vanillaDungeonFlow.rarity = Mathf.RoundToInt(Mathf.Lerp(0, vanillaDungeonFlow.rarity, LethalLevelLoaderPlugin.Instance.scaleDownVanillaDungeonFlowRarityIfCustomDungeonFlowHasChance.Value));
                DebugHelper.Log("PostConfig " + vanillaDungeonFlow.extendedDungeonFlow.name + " , " + vanillaDungeonFlow.rarity);*/

                returnExtendedDungeonFlowsList.Add(vanillaDungeonFlow);
            }

            debugString += "Potential DungeonFlows Collected, List Below: " + "\n";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList)
                debugString += debugDungeon.extendedDungeonFlow.name + " - " + debugDungeon.rarity + ", ";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in potentialExtendedDungeonFlowsList)
                debugString += debugDungeon.extendedDungeonFlow.name + " - " + debugDungeon.rarity + ", ";

            debugString += "\n" + "\n";

            debugString += "Level: " + extendedLevel.NumberlessPlanetName + " Level Tags: ";
            foreach (string tag in extendedLevel.levelTags)
                debugString += tag + ", ";

            debugString += "\n";

            int debugCounter = 1;

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in returnExtendedDungeonFlowsList)
                debugString += "\n" + "DungeonFlow " + debugCounter + ". : " + debugDungeon.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + "Based On The Levels DungeonFlowTypes!" + "\n";

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaManualModList(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + "DungeonFlow " + debugCounter + ". : " + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Manual Mods List!" + "\n";
                    debugCounter++;
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaManualLevelList(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + "DungeonFlow " + debugCounter + ". : " + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Manual Levels List!" + "\n";
                    debugCounter++;
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaRoutePrice(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + "DungeonFlow " + debugCounter + ". : " + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Dynamic Route Price Settings!" + "\n";
                    debugCounter++;
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaLevelTags(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + "DungeonFlow " + debugCounter + ". : " + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Level Tags!" + "\n";
                    debugCounter++;
                }
            }

            debugString += "\n" + "Matching DungeonFlows Collected, Count Is: " + returnExtendedDungeonFlowsList.Count + "\n";

            DebugHelper.Log(debugString + "\n");

            return (returnExtendedDungeonFlowsList.ToArray());
        }

        internal static bool MatchViaManualModList(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonDefaultRarity;

            foreach (StringWithRarity stringWithRarity in extendedDungeonFlow.manualContentSourceNameReferenceList)
                if (stringWithRarity.Name.Contains(extendedLevel.contentSourceName))
                {
                    rarity = stringWithRarity.Rarity;
                    return (true);
                }

            return (false);

            //rarity = 300;
            //return (true);
        }

        internal static bool MatchViaManualLevelList(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonDefaultRarity;

            foreach (StringWithRarity stringWithRarity in extendedDungeonFlow.manualPlanetNameReferenceList)
                if (extendedLevel.selectableLevel.PlanetName.SanitizeString().Contains(stringWithRarity.Name.SanitizeString()) || stringWithRarity.Name.SanitizeString().Contains(extendedLevel.selectableLevel.PlanetName.SanitizeString()))
                {
                    rarity = stringWithRarity.Rarity;
                    return (true);
                }

            return (false);
        }

        internal static bool MatchViaRoutePrice(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonDefaultRarity;

            foreach (Vector2WithRarity vectorWithRarity in extendedDungeonFlow.dynamicRoutePricesList)
            {
                if ((extendedLevel.routePrice >= vectorWithRarity.Min) && (extendedLevel.routePrice <= vectorWithRarity.Max))
                {
                    rarity = vectorWithRarity.Rarity;
                    return (true);
                }
            }


            return (false);
        }

        internal static bool MatchViaLevelTags(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonDefaultRarity;

            foreach (string levelTag in extendedLevel.levelTags)
                foreach (StringWithRarity stringWithRarity in extendedDungeonFlow.dynamicLevelTagsList)
                    if (stringWithRarity.Name.Contains(levelTag))
                    {
                        rarity = (int)stringWithRarity.Rarity;
                        return (true);
                    }

            return (false);
        }

        internal static bool TryGetExtendedDungeonFlow(DungeonFlow dungeonFlow, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType levelType = ContentType.Any)
        {
            returnExtendedDungeonFlow = null;
            List<ExtendedDungeonFlow> extendedDungeonFlowsList = new List<ExtendedDungeonFlow>();

            switch (levelType)
            {
                case ContentType.Vanilla:
                    extendedDungeonFlowsList = vanillaDungeonFlowsList;
                    break;
                case ContentType.Custom:
                    extendedDungeonFlowsList = customDungeonFlowsList;
                    break;
                case ContentType.Any:
                    extendedDungeonFlowsList = allExtendedDungeonsList;
                    break;
            }

            foreach (ExtendedDungeonFlow extendedDungeonFlow in extendedDungeonFlowsList)
                if (extendedDungeonFlow.dungeonFlow == dungeonFlow)
                    returnExtendedDungeonFlow = extendedDungeonFlow;

            return (returnExtendedDungeonFlow != null);
        }

        internal static string GetDebugLevelTags(ExtendedLevel extendedLevel)
        {
            string returnString = extendedLevel.NumberlessPlanetName + " Level Tags: ";

            foreach (string tag in extendedLevel.levelTags)
                returnString += tag + ", ";

            return (returnString + "\n");
        }
    }
}

public static class StringMatchingHelpers
{
    public static string SanitizeString(this string input)
    {
        return new string(input.SkipToLetters().RemoveWhitespace().ToLower());
    }

    public static string RemoveWhitespace(this string input)
    {
        return new string(input.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
    }

    public static string SkipToLetters(this string input)
    {
        return new string(input.SkipWhile(c => !char.IsLetter(c)).ToArray());
    }
}