using DunGen.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader
{
    public class DungeonFlow_Patch
    {
        public static List<ExtendedDungeonFlow> allExtendedDungeonsList = new List<ExtendedDungeonFlow>();
        public static List<ExtendedDungeonFlow> vanillaDungeonFlowsList = new List<ExtendedDungeonFlow>();
        public static List<ExtendedDungeonFlow> customDungeonFlowsList = new List<ExtendedDungeonFlow>();

        public static void CreateExtendedDungeonFlow(DungeonFlow dungeon, int defaultRarity, string sourceName, AudioClip firstTimeDungeonAudio = null)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            newExtendedDungeonFlow.dungeonFlow = dungeon;
            newExtendedDungeonFlow.dungeonFirstTimeAudio = firstTimeDungeonAudio;
            newExtendedDungeonFlow.dungeonRarity = defaultRarity;

            AssetBundleLoader.obtainedExtendedDungeonFlowsList.Add(newExtendedDungeonFlow);
        }

        public static void AddExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.Log("Adding Dungeon Flow: " + extendedDungeonFlow.dungeonFlow.name);
            if (extendedDungeonFlow.dungeonType == ContentType.Custom)
                customDungeonFlowsList.Add(extendedDungeonFlow);
            else
                vanillaDungeonFlowsList.Add(extendedDungeonFlow);

            allExtendedDungeonsList.Add(extendedDungeonFlow);
        }

        static string debugString;
        public static ExtendedDungeonFlowWithRarity[] GetValidExtendedDungeonFlows(ExtendedLevel extendedLevel)
        {
            List<ExtendedDungeonFlowWithRarity> potentialExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> returnExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();

            debugString = "\n" + "Trying To Find All Matching DungeonFlows" + "\n";

            if (extendedLevel.allowedDungeonContentTypes == ContentType.Vanilla || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
                foreach (IntWithRarity intWithRarity in extendedLevel.selectableLevel.dungeonFlowTypes)
                    if (RoundManager.Instance.dungeonFlowTypes[intWithRarity.id] != null)
                        if (TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[intWithRarity.id], out ExtendedDungeonFlow outExtendedDungeonFlow, ContentType.Vanilla))
                            potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(outExtendedDungeonFlow, intWithRarity.rarity));



            if (extendedLevel.allowedDungeonContentTypes == ContentType.Custom || extendedLevel.allowedDungeonContentTypes == ContentType.Any)
                foreach (ExtendedDungeonFlow customDungeonFlow in customDungeonFlowsList)
                    potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, customDungeonFlow.dungeonRarity));

            debugString += "Potential DungeonFlows Collected, List Below: " + "\n";

            foreach (ExtendedDungeonFlowWithRarity debugDungeon in potentialExtendedDungeonFlowsList)
                debugString += debugDungeon.extendedDungeonFlow.name + " - " + debugDungeon.rarity + ", ";

            debugString += "\n" + "\n";

            debugString += extendedLevel.NumberlessPlanetName + " Level Tags: ";
            foreach (string tag in extendedLevel.levelTags)
                debugString += tag + ", ";

            debugString += "\n";

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaManualModList(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Manual Mods List!" + "\n";
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaManualLevelList(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Manual Levels List!" + "\n";
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaRoutePrice(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    debugString += "\n" + customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Dynamic Route Price Settings!" + "\n";
                }
            }

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                if (MatchViaLevelTags(extendedLevel, customDungeonFlow.extendedDungeonFlow, out int outRarity) == true)
                {
                    customDungeonFlow.rarity = outRarity;
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
                    potentialExtendedDungeonFlowsList.Remove(customDungeonFlow);
                    
                    debugString += customDungeonFlow.extendedDungeonFlow.dungeonFlow.name + " - Matched " + extendedLevel.NumberlessPlanetName + " With " + customDungeonFlow.extendedDungeonFlow.name + " Based On Level Tags!" + "\n";
                }
            }

            debugString += "\n" + "Matching DungeonFlows Collected, Count Is: " + returnExtendedDungeonFlowsList.Count + "\n";

            DebugHelper.Log(debugString + "\n");

            return (returnExtendedDungeonFlowsList.ToArray());
        }

        public static bool MatchViaManualModList(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonRarity;

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

        public static bool MatchViaManualLevelList(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonRarity;

            foreach (StringWithRarity stringWithRarity in extendedDungeonFlow.manualPlanetNameReferenceList)
                if (stringWithRarity.Name.Contains(extendedLevel.NumberlessPlanetName))
                {
                    rarity = stringWithRarity.Rarity;
                    return (true);
                }

            return (false);
        }

        public static bool MatchViaRoutePrice(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonRarity;

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

        public static bool MatchViaLevelTags(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow, out int rarity)
        {
            rarity = extendedDungeonFlow.dungeonRarity;

            foreach (string levelTag in extendedLevel.levelTags)
                foreach (StringWithRarity stringWithRarity in extendedDungeonFlow.dynamicLevelTagsList)
                    if (stringWithRarity.Name.Contains(levelTag))
                    {
                        rarity = (int)stringWithRarity.Rarity;
                        return (true);
                    }

            return (false);
        }

        public static bool TryGetExtendedDungeonFlow(DungeonFlow dungeonFlow, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType levelType = ContentType.Any)
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

        private static string GetDebugLevelTags(ExtendedLevel extendedLevel)
        {
            string returnString = extendedLevel.NumberlessPlanetName + " Level Tags: ";

            foreach (string tag in extendedLevel.levelTags)
                returnString += tag + ", ";

            return (returnString + "\n");
        }
    }
}
