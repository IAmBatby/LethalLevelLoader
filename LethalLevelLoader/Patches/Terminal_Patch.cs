using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace LethalLevelLoader
{
    public class StringWithEnum
    {
        public string stringValue;
        public Enum enumValue;

        public StringWithEnum(Enum newEnumValue)
        {
            enumValue = newEnumValue;
            stringValue = string.Empty;
        }

        public StringWithEnum(Enum newEnumValue, string newStringValue)
        {
            enumValue = newEnumValue;
            stringValue = newStringValue;
        }
        //public StringWithEnum(Enum newEnumValue, int newEnumIndex) { enumValue = newEnumValue; stringValue = newEnumValue.ToString(); enumIndex = newEnumIndex; }
        //public StringWithEnum(Enum newEnumValue, string newStringValue) { enumValue = newEnumValue; stringValue = newStringValue; enumIndex = Convert.ToInt32(newEnumValue); }
    }
    public class DynamicKeywordGroup
    {
        public TerminalKeyword verbKeyword;
        public List<StringWithEnum> eventNodeEnumPairsList = new List<StringWithEnum>();

        public DynamicKeywordGroup(string newVerbKeywordWord, List<Enum> stringWithEnumValuesList)
        {
            DebugHelper.Log("newVerbKeywordWord 1 Is: " + newVerbKeywordWord);
            verbKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            verbKeyword.word = newVerbKeywordWord;
            verbKeyword.isVerb = true;

            DebugHelper.Log("newVerbKeywordWord 2 Is: " + verbKeyword.word);

            foreach (Enum stringEnum in stringWithEnumValuesList)
                eventNodeEnumPairsList.Add(new StringWithEnum(stringEnum));

            foreach (StringWithEnum nounKeywordWord in eventNodeEnumPairsList)
            {
                if (nounKeywordWord.stringValue == string.Empty)
                    Terminal_Patch.CreateTerminalEventNode(verbKeyword, nounKeywordWord.enumValue.ToString(), eventString: nounKeywordWord.enumValue.GetType().ToString() + "." + nounKeywordWord.enumValue.ToString());
                else
                    Terminal_Patch.CreateTerminalEventNode(verbKeyword, nounKeywordWord.stringValue, eventString: nounKeywordWord.enumValue.GetType().ToString() + "." + nounKeywordWord.enumValue.ToString() + "-" + nounKeywordWord.stringValue);
            }

            Terminal_Patch.Terminal.terminalNodes.allKeywords = Terminal_Patch.Terminal.terminalNodes.allKeywords.AddItem(verbKeyword).ToArray();

            //GenerateTerminalData();
        }

        public DynamicKeywordGroup(string newVerbKeywordWord, List<StringWithEnum> stringWithEnumValuesList)
        {
            DebugHelper.Log("newVerbKeywordWord 1 Is: " + newVerbKeywordWord);
            verbKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            verbKeyword.word = newVerbKeywordWord;
            verbKeyword.isVerb = true;

            DebugHelper.Log("newVerbKeywordWord 2 Is: " + verbKeyword.word);

            eventNodeEnumPairsList = stringWithEnumValuesList;

            foreach (StringWithEnum nounKeywordWord in eventNodeEnumPairsList)
            {
                if (nounKeywordWord.stringValue == string.Empty)
                    Terminal_Patch.CreateTerminalEventNode(verbKeyword, nounKeywordWord.enumValue.ToString(), eventString: nounKeywordWord.enumValue.GetType().ToString() + "." + nounKeywordWord.enumValue.ToString());
                else
                    Terminal_Patch.CreateTerminalEventNode(verbKeyword, nounKeywordWord.stringValue, eventString: nounKeywordWord.enumValue.GetType().ToString() + "." + nounKeywordWord.enumValue.ToString() + "-" + nounKeywordWord.stringValue);
            }

            Terminal_Patch.Terminal.terminalNodes.allKeywords = Terminal_Patch.Terminal.terminalNodes.allKeywords.AddItem(verbKeyword).ToArray();

            //GenerateTerminalData();
        }

        public bool TryGetEnumValue(string terminalNodeEvent, System.Type enumType, out StringWithEnum returnStringWithEnum)
        {
            DebugHelper.Log("Trying To Get Enum Value! TerminalNode TerminalEvent String Is: " + terminalNodeEvent + " , Provided EnumType Is: " + enumType.ToString());

            returnStringWithEnum = null;

            foreach (StringWithEnum stringWithEnum in eventNodeEnumPairsList)
            {
                DebugHelper.Log("Loop Enum Output: " + stringWithEnum.enumValue.GetType().ToString() + "." + stringWithEnum.enumValue.ToString());
                if (stringWithEnum.stringValue == string.Empty)
                {
                    if (terminalNodeEvent.Contains(stringWithEnum.enumValue.GetType().ToString() + "." + stringWithEnum.enumValue.ToString()))
                    {
                        DebugHelper.Log("Matched!");
                        returnStringWithEnum = stringWithEnum;
                        return (true);
                    }
                }
                else
                {
                    if (terminalNodeEvent.Contains(stringWithEnum.enumValue.GetType().ToString() + "." + stringWithEnum.enumValue.ToString() + "-" + stringWithEnum.stringValue))
                    {
                        DebugHelper.Log("Matched!");
                        returnStringWithEnum = stringWithEnum;
                        return (true);
                    }
                }
            }

            return (false);
        }
    }

    public class Terminal_Patch
    {
        private static Terminal _terminal;
        internal static Terminal Terminal
        {
            get
            {
                if (_terminal == null)
                {
                    _terminal = UnityObjectType.FindObjectOfType<Terminal>();
                    if (_terminal == null) DebugHelper.Log("Failed To Grab Terminal Reference!");
                }
                return _terminal;
            }
        }

        internal static MoonsCataloguePage defaultMoonsCataloguePage;
        internal static MoonsCataloguePage currentMoonsCataloguePage;

        //Hardcoded References To Important Base-Game TerminalKeywords;
        internal static TerminalKeyword RouteKeyword => GetTerminalKeywordFromIndex(26);
        internal static TerminalKeyword InfoKeyword => GetTerminalKeywordFromIndex(6);
        internal static TerminalKeyword ConfirmKeyword => GetTerminalKeywordFromIndex(3);
        internal static TerminalKeyword DenyKeyword => GetTerminalKeywordFromIndex(4);
        internal static TerminalKeyword MoonsKeyword => GetTerminalKeywordFromIndex(21);
        //This isn't anywhere easy to grab so we grab it from Vow's Route.
        internal static TerminalNode CancelRouteNode
        {
            get
            {
                if (RouteKeyword != null)
                    return (RouteKeyword.compatibleNouns[0].result.terminalOptions[0].result);
                else
                    return (null);
            }
        }

        internal static DynamicKeywordGroup previewKeywordsGroup;
        internal static DynamicKeywordGroup sortKeywordGroup;
        internal static DynamicKeywordGroup filterKeywordGroup;
        internal static string currentTagFilter;

        internal static void RefreshExtendedLevelGroups()
        {
            /*MoonsCataloguePage newMoonPage = new MoonsCataloguePage(null);
            List<ExtendedLevelGroup> newGroups = new List<ExtendedLevelGroup>(defaultMoonsCataloguePage.extendedLevelGroups);

            newMoonPage.extendedLevelGroups = newGroups;
            SortMoonsCataloguePage(newMoonPage);
            FilterMoonsCataloguePage(newMoonPage);

            currentMoonsCataloguePage = newMoonPage;

            DebugHelper.DebugMoonsCataloguePage(currentMoonsCataloguePage);
            DebugHelper.DebugMoonsCataloguePage(defaultMoonsCataloguePage);*/

            //DebugHelper.Log("DefaultCataloguePage Is: " + defaultMoonsCataloguePage);
            //DebugHelper.Log("DefaultCataloguePage List Count Is: " + defaultMoonsCataloguePage.extendedLevelGroups.Count);

            //DebugHelper.Log("CurrentCataloguePage Is: " + currentMoonsCataloguePage);
            //DebugHelper.Log("CurrentCataloguePage List Count Is: " + currentMoonsCataloguePage.extendedLevelGroups.Count);

            //currentMoonsCataloguePage.extendedLevelGroups.Clear();

            currentMoonsCataloguePage = new MoonsCataloguePage(defaultMoonsCataloguePage.ExtendedLevelGroups);
            SortMoonsCataloguePage(currentMoonsCataloguePage);
            FilterMoonsCataloguePage(currentMoonsCataloguePage);

        }

        internal static string GetMoonsTerminalText()
        {
            string returnString = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company Building   //   Buying at [companyBuyingPercent].\r\n\r\n";
            return (returnString + GetMoonCatalogDisplayListings() + "\r\n");
        }

        internal static void CreateExtendedLevelGroups()
        {
            ExtendedLevelGroup vanillaGroupA = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(0, 3));
            ExtendedLevelGroup vanillaGroupB = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(3, 2));
            ExtendedLevelGroup vanillaGroupC = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(5, 3));

            ExtendedLevelGroup customGroup = new ExtendedLevelGroup(PatchedContent.CustomExtendedLevels);

            defaultMoonsCataloguePage = new MoonsCataloguePage(new List<ExtendedLevelGroup>() { vanillaGroupA, vanillaGroupB, vanillaGroupC, customGroup });
            currentMoonsCataloguePage = new MoonsCataloguePage(new List<ExtendedLevelGroup>());
            RefreshExtendedLevelGroups();
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings()
        {
            string returnString = string.Empty;

            int counter = 0;
            foreach (ExtendedLevelGroup extendedLevelGroup in currentMoonsCataloguePage.ExtendedLevelGroups)
            {
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";

                counter++;

                if (counter != currentMoonsCataloguePage.ExtendedLevelGroups.Count)
                    returnString += "\n";
            }

            if (currentTagFilter != string.Empty && ModSettings.levelPreviewFilterType == FilterInfoType.Tag)
                returnString += "\n" + "____________________________" + "\n" + "PREVIEW: " + ModSettings.levelPreviewInfoType.ToString().ToUpper() + " | " + "SORT: " + ModSettings.levelPreviewSortType.ToString().ToUpper() + " | " + "FILTER: " + currentTagFilter.ToUpper() + "\n";
            else
                returnString += "\n" + "____________________________" + "\n" + "PREVIEW: " + ModSettings.levelPreviewInfoType.ToString().ToUpper() + " | " + "SORT: " + ModSettings.levelPreviewSortType.ToString().ToUpper() + " | " + "FILTER: " + ModSettings.levelPreviewFilterType.ToString().ToUpper() + "\n";

            return (returnString);
        }

        internal static string GetExtendedLevelPreviewInfo(ExtendedLevel extendedLevel)
        {
            string levelPreviewInfo = string.Empty;

            if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.Weather))
                levelPreviewInfo = GetWeatherConditions(extendedLevel.selectableLevel);
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.Price))
                levelPreviewInfo = "(" + extendedLevel.RoutePrice + ")";
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.Difficulty))
                levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ")";
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.History))
                levelPreviewInfo = GetHistoryConditions(extendedLevel);
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.All))
                levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ") " + "(" + extendedLevel.RoutePrice + ") " + GetWeatherConditions(extendedLevel.selectableLevel);
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.Vanilla))
                levelPreviewInfo = "[planetTime]";
            else if (ModSettings.levelPreviewInfoType.Equals(PreviewInfoType.Override))
                levelPreviewInfo = ModSettings.GetOverridePreviewInfo(extendedLevel);

            return (levelPreviewInfo);
        }

        //Just returns the level weather with a space and ().
        internal static string GetWeatherConditions(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null && selectableLevel.currentWeather != LevelWeatherType.None)
                return ("(" + selectableLevel.currentWeather.ToString() + ")");
            else
                return (string.Empty);
        }

        internal static string GetHistoryConditions(ExtendedLevel extendedLevel)
        {
            bool foundDayHistory = false;

            foreach (DayHistory dayHistory in SelectableLevel_Patch.dayHistoryList)
                if (dayHistory.extendedLevel == extendedLevel)
                {
                    foundDayHistory = true;
                    if (TimeOfDay.Instance.timesFulfilledQuota == dayHistory.quota)
                    {
                        if (SelectableLevel_Patch.daysTotal == dayHistory.day)
                            return ("(Explored Yesterday)");
                        else
                            return ("(Explored " + (SelectableLevel_Patch.daysTotal - dayHistory.day) + " Ago)");
                    }
                    else if ((TimeOfDay.Instance.timesFulfilledQuota - 1) == dayHistory.quota)
                        return ("(Explored Last Quota)");
                    else
                        return ("Explored " + (TimeOfDay.Instance.timesFulfilledQuota - dayHistory.quota) + " Quota's Ago)");
                }

            if (foundDayHistory == false)
                return ("(Unexplored)");
            else
                return string.Empty;
        }

        internal static void CreateLevelTerminalData(ExtendedLevel extendedLevel, int routePrice, out TerminalNode newRouteNode)
        {
            Debug.Log("RouteKeyword Is: " + RouteKeyword);
            //Terminal Route Keyword
            TerminalKeyword terminalKeyword = CreateNewTerminalKeyword();
            terminalKeyword.name = extendedLevel.NumberlessPlanetName;
            terminalKeyword.word = extendedLevel.NumberlessPlanetName.ToLower();
            terminalKeyword.defaultVerb = RouteKeyword;

            //Terminal Route Node
            TerminalNode terminalNodeRoute = CreateNewTerminalNode();
            terminalNodeRoute.name = extendedLevel.NumberlessPlanetName.ToLower() + "Route";
            terminalNodeRoute.displayText = "The cost to route to " + extendedLevel.selectableLevel.PlanetName + " is [totalCost]. It is currently [currentPlanetTime] on this moon.";
            terminalNodeRoute.displayText += "\n" + "\n" + "Please CONFIRM or DENY." + "\n" + "\n";
            terminalNodeRoute.clearPreviousText = true;
            terminalNodeRoute.buyRerouteToMoon = -2;
            terminalNodeRoute.displayPlanetInfo = extendedLevel.selectableLevel.levelID;
            terminalNodeRoute.itemCost = routePrice;
            terminalNodeRoute.overrideOptions = true;

            //Terminal Route Confirm Node
            TerminalNode terminalNodeRouteConfirm = CreateNewTerminalNode();
            terminalNodeRouteConfirm.name = extendedLevel.NumberlessPlanetName.ToLower() + "RouteConfirm";
            terminalNodeRouteConfirm.displayText = "Routing autopilot to " + extendedLevel.selectableLevel.PlanetName + " Your new balance is [playerCredits].";
            terminalNodeRouteConfirm.clearPreviousText = true;
            terminalNodeRouteConfirm.buyRerouteToMoon = extendedLevel.selectableLevel.levelID;
            terminalNodeRouteConfirm.displayPlanetInfo = -1;
            terminalNodeRouteConfirm.itemCost = routePrice;

            //Building The Terminal Node Info String

            string infoString = string.Empty;

            /*infoString += extendedLevel.selectableLevel.PlanetName + "\n";
            infoString += "----------------------" + "\n" + "\n";
            int subStringIndex = extendedLevel.selectableLevel.LevelDescription.IndexOf("CONDITIONS");
            string modifiedDescription = extendedLevel.selectableLevel.LevelDescription.Substring(subStringIndex);
            subStringIndex = modifiedDescription.IndexOf("FAUNA");
            modifiedDescription = modifiedDescription.Insert(subStringIndex, "\n");

            infoString += modifiedDescription;*/

            //Terminal Info Node
            TerminalNode terminalNodeInfo = CreateNewTerminalNode();
            terminalNodeInfo.name = extendedLevel.NumberlessPlanetName.ToLower() + "Info";
            terminalNodeInfo.displayText = infoString;
            terminalNodeInfo.clearPreviousText = true;

            //Population Into Basegame

            terminalNodeRoute.AddCompatibleNoun(DenyKeyword, CancelRouteNode);
            terminalNodeRoute.AddCompatibleNoun(ConfirmKeyword, terminalNodeRouteConfirm);
            RouteKeyword.AddCompatibleNoun(terminalKeyword, terminalNodeRoute);
            InfoKeyword.AddCompatibleNoun(terminalKeyword, terminalNodeInfo);

            DebugHelper.DebugTerminalNode(terminalNodeRoute);
            DebugHelper.DebugTerminalNode(terminalNodeRouteConfirm);
            DebugHelper.DebugTerminalKeyword(terminalKeyword);

            newRouteNode = terminalNodeRoute;
        }

        internal static void CreateMoonsFilterTerminalAssets()
        {
            //Preview & Sort Keywords
            previewKeywordsGroup = new DynamicKeywordGroup("preview", new List<Enum>() { PreviewInfoType.Price, PreviewInfoType.Difficulty, PreviewInfoType.Weather, PreviewInfoType.History, PreviewInfoType.All, PreviewInfoType.None });
            sortKeywordGroup = new DynamicKeywordGroup("sort", new List<Enum>() { SortInfoType.Price, SortInfoType.Difficulty, SortInfoType.None });

            //Tag Keywords
            List<StringWithEnum> tagStringWithEnumsList = new List<StringWithEnum>() { new StringWithEnum(FilterInfoType.Price), new StringWithEnum(FilterInfoType.Weather), new StringWithEnum(FilterInfoType.None) };

            foreach (string levelTag in PatchedContent.AllExtendedLevelTags)
                tagStringWithEnumsList.Add(new StringWithEnum(FilterInfoType.Tag, levelTag));

            filterKeywordGroup = new DynamicKeywordGroup("filter", tagStringWithEnumsList);

            //Simulate Keywords
            List<StringWithEnum> simulateMoonsKeywords = new List<StringWithEnum>();

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                simulateMoonsKeywords.Add(new StringWithEnum(extendedLevel.levelType, extendedLevel.NumberlessPlanetName));

            new DynamicKeywordGroup("simulate", simulateMoonsKeywords);
        }

        internal static bool RunLethalLevelLoaderTerminalEvents(TerminalNode node)
        {
            bool requiresRefresh = false;

            if (node != null && string.IsNullOrEmpty(node.terminalEvent) == false)
            {
                DebugHelper.Log("Running LLL Terminal Event: " + node.terminalEvent);



                if (previewKeywordsGroup.TryGetEnumValue(node.terminalEvent, typeof(PreviewInfoType), out StringWithEnum previewStringWithEnum))
                {
                    ModSettings.levelPreviewInfoType = (PreviewInfoType)previewStringWithEnum.enumValue;
                    requiresRefresh = true;
                }

                else if (sortKeywordGroup.TryGetEnumValue(node.terminalEvent, typeof(SortInfoType), out StringWithEnum sortStringWithEnum))
                {
                    ModSettings.levelPreviewSortType = (SortInfoType)sortStringWithEnum.enumValue;
                    requiresRefresh = true;
                }

                else if (filterKeywordGroup.TryGetEnumValue(node.terminalEvent, typeof(FilterInfoType), out StringWithEnum filterStringWithEnum))
                {
                    currentTagFilter = string.Empty;
                    ModSettings.levelPreviewFilterType = (FilterInfoType)filterStringWithEnum.enumValue;
                    currentTagFilter = filterStringWithEnum.stringValue;
                    requiresRefresh = true;
                }
                else
                {
                    foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                        if (node.terminalEvent.ToLower().Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                        {
                            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = new List<ExtendedDungeonFlowWithRarity>(DungeonFlow_Patch.GetValidExtendedDungeonFlows(extendedLevel, false).OrderBy(o => -(o.rarity)).ToList());
                            string overrideString = "Simulating arrival to " + extendedLevel.selectableLevel.PlanetName + "\n";
                            overrideString += "Analyzing potential remnants found on surface. " + "\n";
                            overrideString += "Listing generated probabilities below." + "\n" + "____________________________" + "\n" + "\n";
                            overrideString += "POSSIBLE STRUCTURES:" + "\n";
                            int totalRarityPool = 0;
                            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                                totalRarityPool += extendedDungeonFlowResult.rarity;
                            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                                overrideString += "* " + extendedDungeonFlowResult.extendedDungeonFlow.dungeonDisplayName + "  //  Chance: " + ((float)extendedDungeonFlowResult.rarity / (float)totalRarityPool * 100).ToString("F2") + "%" + "\n";
                            node.displayText = overrideString + "\n" + "\n";
                            node.clearPreviousText = true;
                            node.isConfirmationNode = true;
                            return (true);
                        }
                }
                if (requiresRefresh == true)
                {
                    RefreshExtendedLevelGroups();

                    string updatedDisplayText = "\n" + "\n" + "\n" + GetMoonsTerminalText();
                    updatedDisplayText = Terminal.TextPostProcess(updatedDisplayText, Terminal.currentNode);

                    Terminal.screenText.text = updatedDisplayText;
                    Terminal.currentText = updatedDisplayText;

                    return (false);
                }
            }

            return (true);
        }

        internal static void FilterMoonsCataloguePage(MoonsCataloguePage moonsCataloguePage)
        {
            List<ExtendedLevelGroup> moonsCatalogueGroups = new List<ExtendedLevelGroup>(moonsCataloguePage.ExtendedLevelGroups);

            foreach (ExtendedLevelGroup extendedLevelGroup in new List<ExtendedLevelGroup>(moonsCatalogueGroups))
                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(extendedLevelGroup.extendedLevelsList))
                {
                    bool removeExtendedLevel = false;

                    if (ModSettings.levelPreviewFilterType.Equals(FilterInfoType.Price))
                        removeExtendedLevel = (extendedLevel.RoutePrice > Terminal.groupCredits);
                    else if (ModSettings.levelPreviewFilterType.Equals(FilterInfoType.Weather))
                        removeExtendedLevel = (GetWeatherConditions(extendedLevel.selectableLevel) != string.Empty);
                    else if (ModSettings.levelPreviewFilterType.Equals(FilterInfoType.Tag))
                        removeExtendedLevel = (!extendedLevel.levelTags.Contains(currentTagFilter));

                    if (removeExtendedLevel == true)
                        extendedLevelGroup.extendedLevelsList.Remove(extendedLevel);
                }

            moonsCataloguePage.RebuildLevelGroups(moonsCatalogueGroups, 3);
        }

        internal static void SortMoonsCataloguePage(MoonsCataloguePage cataloguePage)
        {
            Debug.Log("Sorting Moons!");
            if (ModSettings.levelPreviewSortType.Equals(SortInfoType.Price))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.RoutePrice), 3);
            else if (ModSettings.levelPreviewSortType.Equals(SortInfoType.Difficulty))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount), 3);
            Debug.Log("Sorted Moons!");
        }

        internal static (TerminalKeyword, TerminalNode) CreateTerminalEventNode(TerminalKeyword verbKeyword, string nounWord, string eventString = null)
        {
            DebugHelper.Log("Creating New TerminalEvent Node! VerbKeyword Word Is: " + verbKeyword.word + " | nounWord Is: " + nounWord + " | EventString Is: " + eventString);
            TerminalKeyword newKeyword = CreateNewTerminalKeyword();
            TerminalNode newNode = CreateNewTerminalNode();

            if (eventString == null)
                eventString = nounWord;

            newKeyword.word = nounWord.ToLower();
            newKeyword.defaultVerb = verbKeyword;
            newNode.terminalEvent = eventString;

            verbKeyword.AddCompatibleNoun(newKeyword, newNode);

            return (newKeyword, newNode);
        }


        //Nani the fuck?
        internal static TerminalKeyword GetTerminalKeywordFromIndex(int index)
        {
            if (Terminal != null)
                return (Terminal.terminalNodes.allKeywords[index]);
            else
                return (null);
        }

        internal static TerminalKeyword CreateNewTerminalKeyword()
        {
            TerminalKeyword newTerminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            newTerminalKeyword.compatibleNouns = new CompatibleNoun[0];
            newTerminalKeyword.name = string.Empty;
            newTerminalKeyword.word = string.Empty;
            newTerminalKeyword.defaultVerb = null;

            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(newTerminalKeyword).ToArray();

            return (newTerminalKeyword);
        }

        internal static TerminalNode CreateNewTerminalNode()
        {
            TerminalNode newTerminalNode = ScriptableObject.CreateInstance<TerminalNode>();

            newTerminalNode.name = string.Empty;
            newTerminalNode.displayText = string.Empty;
            newTerminalNode.clearPreviousText = false;
            newTerminalNode.maxCharactersToType = 25;
            newTerminalNode.terminalEvent = string.Empty;
            newTerminalNode.buyItemIndex = -1;
            newTerminalNode.buyRerouteToMoon = -1;
            newTerminalNode.displayPlanetInfo = -1;
            newTerminalNode.shipUnlockableID = -1;
            newTerminalNode.itemCost = 0;
            newTerminalNode.creatureFileID = -1;
            newTerminalNode.storyLogFileID = -1;
            newTerminalNode.overrideOptions = false;
            newTerminalNode.playSyncedClip = -1;
            newTerminalNode.terminalOptions = new CompatibleNoun[0];

            return (newTerminalNode);
        }
    }
}