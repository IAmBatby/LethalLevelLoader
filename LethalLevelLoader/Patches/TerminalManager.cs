using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalLevelLoader
{
    public class TerminalManager
    {
        private static Terminal _terminal;
        internal static Terminal Terminal
        {
            get
            {
                if (_terminal == null)
                    _terminal = UnityObjectType.FindObjectOfType<Terminal>();

                return _terminal;
            }
        }

        internal static TerminalNode lockedNode;

        internal static MoonsCataloguePage defaultMoonsCataloguePage;
        internal static MoonsCataloguePage currentMoonsCataloguePage;

        //Cached References To Important Base-Game TerminalKeywords;
        internal static TerminalKeyword routeKeyword;
        internal static TerminalKeyword infoKeyword;
        internal static TerminalKeyword confirmKeyword;
        internal static TerminalKeyword denyKeyword;
        internal static TerminalKeyword moonsKeyword;
        internal static TerminalKeyword viewKeyword;
        internal static TerminalNode cancelRouteNode;

        internal static string currentTagFilter;

        internal static TerminalKeyword lastParsedVerbKeyword;

        public delegate string PreviewInfoText(ExtendedLevel extendedLevel, PreviewInfoType infoType);
        public static event PreviewInfoText onBeforePreviewInfoTextAdded;

        ////////// Setting Data //////////

        internal static void CacheTerminalReferences()
        {
            routeKeyword = Terminal.terminalNodes.allKeywords[26];
            infoKeyword = Terminal.terminalNodes.allKeywords[6];
            confirmKeyword = Terminal.terminalNodes.allKeywords[3];
            denyKeyword = Terminal.terminalNodes.allKeywords[4];
            moonsKeyword = Terminal.terminalNodes.allKeywords[21];
            viewKeyword = Terminal.terminalNodes.allKeywords[19];
            cancelRouteNode = routeKeyword.compatibleNouns[0].result.terminalOptions[0].result;

            lockedNode = CreateNewTerminalNode();
            lockedNode.name = "lockedLevelNode";
            lockedNode.clearPreviousText = true;
        }

        internal static void SwapRouteNodeToLockedNode(ExtendedLevel extendedLevel, ref TerminalNode terminalNode)
        {
            if (extendedLevel.lockedNodeText != string.Empty)
                lockedNode.displayText = extendedLevel.lockedNodeText;
            else
                lockedNode.displayText = "Route to " + extendedLevel.selectableLevel.PlanetName + " is currently locked.";

            terminalNode = lockedNode;
        }
        
        internal static void RefreshExtendedLevelGroups()
        {
            currentMoonsCataloguePage = new MoonsCataloguePage(defaultMoonsCataloguePage.ExtendedLevelGroups);
            if (Settings.levelPreviewSortType != SortInfoType.None)
                SortMoonsCataloguePage(currentMoonsCataloguePage);
            FilterMoonsCataloguePage(currentMoonsCataloguePage);
        }

        internal static bool RunLethalLevelLoaderTerminalEvents(TerminalNode node)
        {
            if (node != null && string.IsNullOrEmpty(node.terminalEvent) == false)
            {
                //DebugHelper.Log("Running LLL Terminal Event: " + node.terminalEvent + "| EnumValue: " + GetTerminalEventEnum(node.terminalEvent) + " | StringValue: " + GetTerminalEventString(node.terminalEvent));
                if (node.name.Contains("preview") && Enum.TryParse(typeof(PreviewInfoType), GetTerminalEventEnum(node.terminalEvent), out object previewEnumValue))
                    Settings.levelPreviewInfoType = (PreviewInfoType)previewEnumValue;
                else if (node.name.Contains("sort") && Enum.TryParse(typeof(SortInfoType), GetTerminalEventEnum(node.terminalEvent), out object sortEnumValue))
                    Settings.levelPreviewSortType = (SortInfoType)sortEnumValue;
                else if (node.name.Contains("filter") && Enum.TryParse(typeof(FilterInfoType), GetTerminalEventEnum(node.terminalEvent), out object filterEnumValue))
                {
                    Settings.levelPreviewFilterType = (FilterInfoType)filterEnumValue;
                    currentTagFilter = GetTerminalEventString(node.terminalEvent);
                    DebugHelper.Log("Tag EventString: " + GetTerminalEventString(node.terminalEvent));
                }
                else
                {
                    foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                        if (node.terminalEvent.ToLower().Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                        {
                            node.displayText = GetSimulationResultsText(extendedLevel) + "\n" + "\n";
                            node.clearPreviousText = true;
                            node.isConfirmationNode = true;
                            return (true);
                        }
                }

                RefreshExtendedLevelGroups();

                Terminal.screenText.text = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);
                Terminal.currentText = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);

                return (false);
            }
            return (true);
        }

        internal static void FilterMoonsCataloguePage(MoonsCataloguePage moonsCataloguePage)
        {
            List<ExtendedLevel> removeLevelList = new List<ExtendedLevel>();

            foreach (ExtendedLevelGroup extendedLevelGroup in moonsCataloguePage.ExtendedLevelGroups)
                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(extendedLevelGroup.extendedLevelsList))
                {
                    bool removeExtendedLevel = extendedLevel.isHidden;

                    if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Price))
                        removeExtendedLevel = (extendedLevel.RoutePrice > Terminal.groupCredits);
                    else if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Weather))
                        removeExtendedLevel = (GetWeatherConditions(extendedLevel.selectableLevel) != string.Empty);
                    else if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Tag))
                        removeExtendedLevel = (!extendedLevel.levelTags.Contains(currentTagFilter));

                    if (removeExtendedLevel == true)
                        removeLevelList.Add(extendedLevel);
                }

            foreach (ExtendedLevelGroup extendedLevelGroup in moonsCataloguePage.ExtendedLevelGroups)
                foreach (ExtendedLevel extendedLevel in removeLevelList)
                    if (extendedLevelGroup.extendedLevelsList.Contains(extendedLevel))
                        extendedLevelGroup.extendedLevelsList.Remove(extendedLevel);

            if (Settings.levelPreviewFilterType != FilterInfoType.None)
                moonsCataloguePage.RebuildLevelGroups(new List<ExtendedLevelGroup>(moonsCataloguePage.ExtendedLevelGroups), 3);
        }

        internal static void SortMoonsCataloguePage(MoonsCataloguePage cataloguePage)
        {
            if (Settings.levelPreviewSortType.Equals(SortInfoType.Price))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.RoutePrice), 3);
            else if (Settings.levelPreviewSortType.Equals(SortInfoType.Difficulty))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount), 3);
        }

        ////////// Getting Data //////////

        internal static string GetMoonsTerminalText()
        {
            string returnString = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company Building   //   Buying at [companyBuyingPercent].\r\n\r\n";
            return (returnString + GetMoonCatalogDisplayListings() + "\r\n");
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings()
        {
            string returnString = string.Empty;

            foreach (ExtendedLevelGroup extendedLevelGroup in currentMoonsCataloguePage.ExtendedLevelGroups)
            {
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";
                returnString += "\n";
            }
            returnString.Replace(returnString.Substring(returnString.LastIndexOf("\n")), "");

            string tagString = Settings.levelPreviewFilterType.ToString().ToUpper();
            if (Settings.levelPreviewFilterType == FilterInfoType.Tag)
                tagString = currentTagFilter.ToUpper();

            return (returnString + "\n" + "____________________________" + "\n" + "PREVIEW: " + Settings.levelPreviewInfoType.ToString().ToUpper() + " | " + "SORT: " + Settings.levelPreviewSortType.ToString().ToUpper() + " | " + "FILTER: " + tagString + "\n");
        }

        internal static string GetExtendedLevelPreviewInfo(ExtendedLevel extendedLevel)
        {
            string levelPreviewInfo = string.Empty;

            if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Weather))
                levelPreviewInfo = GetWeatherConditions(extendedLevel.selectableLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Price))
                levelPreviewInfo = "(" + extendedLevel.RoutePrice + ")";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Difficulty))
                levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ")";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.History))
                levelPreviewInfo = GetHistoryConditions(extendedLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.All))
                levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ") " + "(" + extendedLevel.RoutePrice + ") " + GetWeatherConditions(extendedLevel.selectableLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Vanilla))
                levelPreviewInfo = "[planetTime]";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Override))
                levelPreviewInfo = Settings.GetOverridePreviewInfo(extendedLevel);
            if (extendedLevel.isLocked == true)
                levelPreviewInfo += " (Locked)";

            string overridePreviewInfo = onBeforePreviewInfoTextAdded?.Invoke(extendedLevel, Settings.levelPreviewInfoType);
            if (overridePreviewInfo != null && overridePreviewInfo != string.Empty)
                levelPreviewInfo = overridePreviewInfo;

            return (levelPreviewInfo);
        }

        //Just returns the level weather with a space and ().
        internal static string GetWeatherConditions(SelectableLevel selectableLevel)
        {
            string returnString = string.Empty;
            if (selectableLevel != null && selectableLevel.currentWeather != LevelWeatherType.None)
                returnString = "(" + selectableLevel.currentWeather.ToString() + ")";
            return (returnString);
        }

        internal static string GetHistoryConditions(ExtendedLevel extendedLevel)
        {
            DayHistory dayHistory = null;

            foreach (DayHistory loggedDayHistory in LevelManager.dayHistoryList)
                if (loggedDayHistory.extendedLevel == extendedLevel)
                    dayHistory = loggedDayHistory;

            if (dayHistory == null)
                return ("(Unexplored)");
            else if (TimeOfDay.Instance.timesFulfilledQuota == dayHistory.quota && LevelManager.daysTotal == dayHistory.day)
                return ("(Explored Yesterday)");
            else if (TimeOfDay.Instance.timesFulfilledQuota == dayHistory.quota)
                return ("(Explored " + (LevelManager.daysTotal - dayHistory.day) + " Ago)");
            else if ((TimeOfDay.Instance.timesFulfilledQuota - 1) == dayHistory.quota)
                return ("(Explored Last Quota)");
            else
                return ("Explored " + (TimeOfDay.Instance.timesFulfilledQuota - dayHistory.quota) + " Quota's Ago)");
        }

        public static string GetTerminalEventString(string terminalEventString)
        {
            string returnString = string.Empty;
            if (terminalEventString.Contains(";"))
                returnString = terminalEventString.Substring(terminalEventString.IndexOf(";") + 1);
            return (returnString);
        }

        public static string GetTerminalEventEnum(string terminalEventString)
        {
            if (terminalEventString.Contains(";"))
                terminalEventString = terminalEventString.Replace(terminalEventString.Substring(terminalEventString.IndexOf(";")), "");
            return (terminalEventString);
        }


        internal static string GetSimulationResultsText(ExtendedLevel extendedLevel)
        {
            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = new List<ExtendedDungeonFlowWithRarity>(DungeonManager.GetValidExtendedDungeonFlows(extendedLevel, false).OrderBy(o => -(o.rarity)).ToList());
            string overrideString = "Simulating arrival to " + extendedLevel.selectableLevel.PlanetName + "\nAnalyzing potential remnants found on surface. \nListing generated probabilities below.\n____________________________ \n\nPOSSIBLE STRUCTURES: \n";
            int totalRarityPool = 0;
            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                totalRarityPool += extendedDungeonFlowResult.rarity;
            if (extendedLevel.NumberlessPlanetName.Sanitized().Contains("march") && extendedLevel.selectableLevel.dungeonFlowTypes.Length == 0) //Obligitory Fuck March.
            {
                totalRarityPool += 300;
                overrideString += "* " + "Facility" + "  //  Chance: " + GetSimulationDataText(300, totalRarityPool) + "\n";
            }
            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                overrideString += "* " + extendedDungeonFlowResult.extendedDungeonFlow.dungeonDisplayName + "  //  Chance: " + GetSimulationDataText(extendedDungeonFlowResult.rarity, totalRarityPool) + "\n";

            return (overrideString);
        }

        internal static string GetSimulationDataText(int rarity, int totalRarity)
        {
            string returnString = string.Empty;
            if (Settings.levelSimulateInfoType == SimulateInfoType.Percentage)
                returnString = (((float)rarity / (float)totalRarity * 100).ToString("F2") + "%");
            else if (Settings.levelSimulateInfoType == SimulateInfoType.Rarity)
                returnString = (rarity + " // " + totalRarity);
            return (returnString);
        }

        internal static TerminalKeyword TryFindAlternativeNoun(Terminal terminal, TerminalKeyword foundKeyword, string playerInput)
        {
            if (foundKeyword != null & terminal.hasGottenVerb == false && foundKeyword.isVerb == true)
                lastParsedVerbKeyword = foundKeyword;

            if (foundKeyword != null && foundKeyword.isVerb == false && terminal.hasGottenVerb == true && lastParsedVerbKeyword != null)
            {
                TerminalKeyword nounKeyword = foundKeyword;
                if (ValidateNounKeyword(lastParsedVerbKeyword, nounKeyword) == false)
                    foreach (TerminalKeyword newNounKeyword in Terminal.terminalNodes.allKeywords)
                        if (newNounKeyword.isVerb == false && newNounKeyword != nounKeyword && newNounKeyword.word == playerInput)
                            if (ValidateNounKeyword(lastParsedVerbKeyword, newNounKeyword) == true)
                            {
                                lastParsedVerbKeyword = null;
                                return (newNounKeyword);
                            }
            }

            //DebugHelper.Log("Returning TerminalKeyword: " + foundKeyword.word);
            return (foundKeyword);
        }

        internal static bool ValidateNounKeyword(TerminalKeyword verbKeyword, TerminalKeyword nounKeyword)
        {
            for (int k = 0; k < verbKeyword.compatibleNouns.Length; k++)
                if (verbKeyword.compatibleNouns[k].noun == nounKeyword)
                    return (true);
            return (false);
        }

        ////////// Creating Data //////////

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

        internal static void CreateLevelTerminalData(ExtendedLevel extendedLevel, int routePrice)
        {
            //Terminal Route Keyword
            TerminalKeyword terminalKeyword = CreateNewTerminalKeyword();
            terminalKeyword.name = extendedLevel.NumberlessPlanetName;
            terminalKeyword.word = extendedLevel.NumberlessPlanetName.ToLower();
            terminalKeyword.defaultVerb = routeKeyword;

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
            terminalNodeRouteConfirm.itemCost = routePrice;

            //Terminal Info Node
            TerminalNode terminalNodeInfo = CreateNewTerminalNode();
            terminalNodeInfo.name = extendedLevel.NumberlessPlanetName.ToLower() + "Info";
            terminalNodeInfo.clearPreviousText = true;
            terminalNodeInfo.maxCharactersToType = 35;


            string infoString = extendedLevel.selectableLevel.PlanetName + "\n" + "----------------------" + "\n";
            List<string> selectableLevelLines = new List<string>();

            string inputString;
            if (extendedLevel.infoNodeDescripton != string.Empty)
                inputString = extendedLevel.infoNodeDescripton;
            else
            inputString = extendedLevel.selectableLevel.LevelDescription;

            while (inputString.Contains("\n"))
            {
                string inputStringWithoutTextBeforeFirstComma = inputString.Substring(inputString.IndexOf("\n"));
                selectableLevelLines.Add(inputString.Replace(inputStringWithoutTextBeforeFirstComma, ""));
                if (inputStringWithoutTextBeforeFirstComma.Contains("\n"))
                    inputString = inputStringWithoutTextBeforeFirstComma.Substring(inputStringWithoutTextBeforeFirstComma.IndexOf("\n") + 1);
            }
            selectableLevelLines.Add(inputString);

            foreach (string line in selectableLevelLines)
                infoString += "\n" + line + "\n";

            terminalNodeInfo.displayText = infoString;

            foreach (StoryLogData newStoryLog in extendedLevel.storyLogs)
                if (newStoryLog.terminalWord != string.Empty && newStoryLog.storyLogTitle != string.Empty && newStoryLog.storyLogDescription != string.Empty)
                {
                    TerminalKeyword newStoryLogKeyword = CreateNewTerminalKeyword();
                    newStoryLogKeyword.word = newStoryLog.terminalWord;
                    newStoryLogKeyword.name = newStoryLog.terminalWord + "Keyword";
                    newStoryLogKeyword.defaultVerb = viewKeyword;
                    TerminalNode newStoryLogNode = CreateNewTerminalNode();
                    newStoryLogNode.name = newStoryLog.terminalWord + "Node";
                    newStoryLogNode.clearPreviousText = true;
                    newStoryLogNode.creatureName = newStoryLog.storyLogTitle;
                    newStoryLogNode.storyLogFileID = Terminal.logEntryFiles.Count;
                    newStoryLog.newStoryLogID = Terminal.logEntryFiles.Count;

                    Terminal.logEntryFiles.Add(newStoryLogNode);
                    viewKeyword.AddCompatibleNoun(newStoryLogKeyword, newStoryLogNode);
                }


            //Population Into Basegame

            terminalNodeRoute.AddCompatibleNoun(denyKeyword, cancelRouteNode);
            terminalNodeRoute.AddCompatibleNoun(confirmKeyword, terminalNodeRouteConfirm);
            routeKeyword.AddCompatibleNoun(terminalKeyword, terminalNodeRoute);
            infoKeyword.AddCompatibleNoun(terminalKeyword, terminalNodeInfo);

            if (extendedLevel.levelType == ContentType.Custom)
            {
                extendedLevel.routeNode = terminalNodeRoute;
                extendedLevel.routeConfirmNode = terminalNodeRouteConfirm;
                extendedLevel.infoNode = terminalNodeInfo;
            }
        }

        internal static void RegisterStoryLog(TerminalKeyword terminalKeyword, TerminalNode terminalNode)
        {

        }

        internal static void CreateMoonsFilterTerminalAssets()
        {
            //Preview & Sort Keywords
            CreateTerminalEventNodes("preview", new List<Enum>() { PreviewInfoType.Price, PreviewInfoType.Difficulty, PreviewInfoType.Weather, PreviewInfoType.History, PreviewInfoType.All, PreviewInfoType.None });
            CreateTerminalEventNodes("sort", new List<Enum>() { SortInfoType.Price, SortInfoType.Difficulty, SortInfoType.None });
            CreateTerminalEventNodes("filter", new List<Enum>() { FilterInfoType.Price, FilterInfoType.Weather, FilterInfoType.None });
            //Tag Keywords
            List<string> tagMoonWordsList = new List<string>();
            List<string> tagMoonTerminalEventsList = new List<string>();
            foreach (string levelTag in PatchedContent.AllExtendedLevelTags)
            {
                tagMoonWordsList.Add(levelTag);
                tagMoonTerminalEventsList.Add("Tag;" + levelTag);
            }

            CreateTerminalEventNodes("filter", tagMoonWordsList, tagMoonTerminalEventsList, createNewVerbKeyword: false);

            //Simulate Keywords
            List<string> simulateMoonsKeywords = new List<string>();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                simulateMoonsKeywords.Add(extendedLevel.NumberlessPlanetName);

            CreateTerminalEventNodes("simulate", simulateMoonsKeywords);
        }

        internal static void CreateTerminalEventNodes(string newVerbKeywordWord, List<Enum> terminalEventEnumStrings)
        {
            List<string> convertedList = new List<string>();
            foreach (Enum enumValue in terminalEventEnumStrings)
                convertedList.Add(enumValue.ToString());
            CreateTerminalEventNodes(newVerbKeywordWord, convertedList);
        }

        internal static void CreateTerminalEventNodes(string newVerbKeywordWord, List<string> nounWords, List<string> terminalEventStrings = null, bool createNewVerbKeyword = true)
        {
            TerminalKeyword verbKeyword = null;
            if (createNewVerbKeyword == true)
                verbKeyword = CreateNewTerminalKeyword();
            else
                foreach (TerminalKeyword terminalKeyword in Terminal.terminalNodes.allKeywords)
                    if (terminalKeyword.isVerb == true && terminalKeyword.word == newVerbKeywordWord.ToLower())
                        verbKeyword = terminalKeyword;  
            verbKeyword.word = newVerbKeywordWord.ToLower();
            verbKeyword.name = newVerbKeywordWord.ToLower() + "Keyword";
            verbKeyword.isVerb = true;

            if (terminalEventStrings == null)
                terminalEventStrings = nounWords;

            foreach (string newNode in nounWords)
                    CreateTerminalEventNode(verbKeyword, newNode, terminalEventStrings[nounWords.IndexOf(newNode)]);
        }

        internal static void CreateTerminalEventNode(TerminalKeyword verbKeyword, string nounWord, string terminalEventString)
        {
            //DebugHelper.Log("Creating New TerminalEvent Node! VerbKeyword Word Is: " + verbKeyword.word + " | nounWord Is: " + GetTerminalEventEnum(nounWord).ToLower() + " | TerminalEvent Text Is: " + terminalEventString);
            TerminalKeyword newKeyword = CreateNewTerminalKeyword();
            TerminalNode newNode = CreateNewTerminalNode();

            newKeyword.name = verbKeyword.word + GetTerminalEventEnum(nounWord) + "Keyword";
            newKeyword.word = GetTerminalEventEnum(nounWord).ToLower();
            newKeyword.defaultVerb = verbKeyword;
            newNode.terminalEvent = terminalEventString;
            newNode.name = verbKeyword.word + GetTerminalEventEnum(nounWord) + "Node";

            verbKeyword.AddCompatibleNoun(newKeyword, newNode);
        }

        internal static TerminalKeyword CreateNewTerminalKeyword()
        {
            TerminalKeyword newTerminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            newTerminalKeyword.compatibleNouns = new CompatibleNoun[0];
            newTerminalKeyword.defaultVerb = null;
            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(newTerminalKeyword).ToArray();

            return (newTerminalKeyword);
        }

        internal static TerminalNode CreateNewTerminalNode()
        {
            TerminalNode newTerminalNode = ScriptableObject.CreateInstance<TerminalNode>();

            newTerminalNode.displayText = string.Empty;
            newTerminalNode.terminalEvent = string.Empty;
            newTerminalNode.maxCharactersToType = 25;
            newTerminalNode.buyItemIndex = -1;
            newTerminalNode.buyRerouteToMoon = -1;
            newTerminalNode.displayPlanetInfo = -1;
            newTerminalNode.shipUnlockableID = -1;
            newTerminalNode.creatureFileID = -1;
            newTerminalNode.storyLogFileID = -1;
            newTerminalNode.playSyncedClip = -1;
            newTerminalNode.terminalOptions = new CompatibleNoun[0];

            return (newTerminalNode);
        }
    }
}