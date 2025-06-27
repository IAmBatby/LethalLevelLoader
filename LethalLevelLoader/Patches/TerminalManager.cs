#if !HARMONY_DISABLED
using HarmonyLib;
using JetBrains.Annotations;
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

        public static MoonsCataloguePage defaultMoonsCataloguePage { get; internal set; }
        public static MoonsCataloguePage currentMoonsCataloguePage { get; internal set; }

        //Cached References To Important Base-Game TerminalKeywords;
        internal static TerminalKeyword Keyword_Route;
        internal static TerminalKeyword Keyword_Info;
        internal static TerminalKeyword Keyword_Confirm;
        internal static TerminalKeyword Keyword_Deny;
        internal static TerminalKeyword Keyword_Moons;
        internal static TerminalKeyword Keyword_View;
        internal static TerminalKeyword Keyword_Buy;
        internal static TerminalNode Node_CancelRoute;
        internal static TerminalNode Node_CancelPurchase;

        internal static string currentTagFilter;

        public static float defaultTerminalFontSize;

        internal static TerminalKeyword lastParsedVerbKeyword;

        public delegate string PreviewInfoText(ExtendedLevel extendedLevel, PreviewInfoType infoType);
        public static event PreviewInfoText onBeforePreviewInfoTextAdded;

        //internal static Dictionary<TerminalNode, Action<TerminalNode, TerminalNode>> terminalNodeRegisteredEventDictionary = new Dictionary<TerminalNode, Action<TerminalNode, TerminalNode>>();

        public enum LoadNodeActionType { Before,  After }
        public delegate bool LoadNodeAction(ref TerminalNode currentNode, ref TerminalNode loadNode);

        internal static Dictionary<TerminalNode, LoadNodeAction> onBeforeLoadNewNodeRegisteredEventsDictionary = new Dictionary<TerminalNode, LoadNodeAction>();
        internal static Dictionary<TerminalNode, LoadNodeAction> onLoadNewNodeRegisteredEventsDictionary = new Dictionary<TerminalNode, LoadNodeAction>();

        ////////// Setting Data //////////

        internal static void CacheTerminalReferences()
        {
            Keyword_Route = Terminal.terminalNodes.allKeywords[27];
            Keyword_Info = Terminal.terminalNodes.allKeywords[6];
            Keyword_Confirm = Terminal.terminalNodes.allKeywords[3];
            Keyword_Deny = Terminal.terminalNodes.allKeywords[4];
            Keyword_Moons = Terminal.terminalNodes.allKeywords[21];
            Keyword_View = Terminal.terminalNodes.allKeywords[19];
            Keyword_Buy = Terminal.terminalNodes.allKeywords[0];
            Node_CancelRoute = Keyword_Route.compatibleNouns[0].result.terminalOptions[0].result;
            Node_CancelPurchase = Keyword_Buy.compatibleNouns[0].result.terminalOptions[1].result;

            defaultTerminalFontSize = Terminal.screenText.textComponent.fontSize;

            lockedNode = CreateNewTerminalNode();
            lockedNode.name = "lockedLevelNode";
            lockedNode.acceptAnything = false;
            lockedNode.clearPreviousText = true;
        }

        internal static bool OnBeforeRouteNodeLoaded(ref TerminalNode currentNode, ref TerminalNode loadNode)
        {
            TerminalNode confirmNode = loadNode.terminalOptions[1].result;
            if (confirmNode.buyRerouteToMoon < 0 || confirmNode.buyRerouteToMoon > StartOfRound.Instance.levels.Length - 1)
            {
                DebugHelper.LogError("Invalid DisplayPlanetInfo For Route Node: " + confirmNode.name, DebugType.User);
                return (true);
            }
            ExtendedLevel extendedLevel = LevelManager.GetExtendedLevel(StartOfRound.Instance.levels[confirmNode.buyRerouteToMoon]);

            if (extendedLevel == null)
            {
                DebugHelper.LogError("ExtendedLevel Was Null For Route Node: " + confirmNode.name, DebugType.User);
                return (true);
            }
            if (currentNode != null)
                DebugHelper.Log("LockedNodeEventTest: ExtendedLevel Is: " + extendedLevel + ", CurrentNode Is: " + currentNode.name + ", LoadNode Is: " + confirmNode.name, DebugType.User);
            else
                DebugHelper.Log("LockedNodeEventTest: ExtendedLevel Is: " + extendedLevel + ", CurrentNode Is Null, LoadNode Is: " + confirmNode.name, DebugType.User);

            if (extendedLevel.IsRouteLocked == true)
                SwapRouteNodeToLockedNode(extendedLevel, ref loadNode);
            return (true);
        }

        internal static void SwapRouteNodeToLockedNode(ExtendedLevel extendedLevel, ref TerminalNode terminalNode)
        {
            if (extendedLevel.LockedRouteNodeText != string.Empty)
                lockedNode.displayText = extendedLevel.LockedRouteNodeText + "\n\n\n";
            else
                lockedNode.displayText = "Route to " + extendedLevel.SelectableLevel.PlanetName + " is currently locked." + "\n\n\n";

            terminalNode = lockedNode;
        }
        
        internal static void RefreshExtendedLevelGroups()
        {
            currentMoonsCataloguePage.ExtendedLevelGroups.Clear();
            currentMoonsCataloguePage = new MoonsCataloguePage(defaultMoonsCataloguePage.ExtendedLevelGroups);
            if (Settings.levelPreviewSortType != SortInfoType.None)
                SortMoonsCataloguePage(currentMoonsCataloguePage);
            FilterMoonsCataloguePage(currentMoonsCataloguePage);
        }

        internal static bool SetSimulationResultsText(ref TerminalNode currentNode, ref TerminalNode node)
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                if (node.terminalEvent.StripSpecialCharacters().Sanitized().ToLower().Contains(extendedLevel.NumberlessPlanetName.StripSpecialCharacters().Sanitized().ToLower()))
                {
                    node.displayText = GetSimulationResultsText(extendedLevel) + "\n" + "\n";
                    node.clearPreviousText = true;
                    node.isConfirmationNode = true;
                }

            return (true);
        }

        internal static bool OnBeforeLoadNewNode(ref TerminalNode node)
        {
            if (onBeforeLoadNewNodeRegisteredEventsDictionary.TryGetValue(node, out LoadNodeAction pair))
            {
                DebugHelper.Log("Running OnBeforeLoadNewNode Event For: " + node.name + ", CurrentNode Is: " + Terminal.currentNode, DebugType.Developer);
                return (pair.Invoke(ref Terminal.currentNode, ref node));
            }
            else
            {
                DebugHelper.Log("Could Not Find Registered Event For: " + node.name, DebugType.Developer);
                return (true);
            }
        }

        internal static void OnLoadNewNode(ref TerminalNode node)
        {
            if (onLoadNewNodeRegisteredEventsDictionary.TryGetValue(node, out LoadNodeAction pair))
            {
                DebugHelper.Log("Running OnLoadNewNode Event For: " + node.name + ", CurrentNode Is: " + Terminal.currentNode, DebugType.Developer);
                pair.Invoke(ref Terminal.currentNode, ref node);
            }
            else
                DebugHelper.Log("Could Not Find Registered Event For: " + node.name, DebugType.Developer);
        }

        internal static bool RunLethalLevelLoaderTerminalEvents(TerminalNode node)
        {
            /*if (node != null && string.IsNullOrEmpty(node.terminalEvent) == false)
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

                RefreshExtendedLevelGroups();

                Terminal.screenText.text = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);
                Terminal.currentText = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);

                return (false);
            }*/
            return (true);
        }

        internal static bool TryRefreshMoonsCataloguePage(ref TerminalNode currentNode, ref TerminalNode loadNode)
        {
            if (currentNode == Keyword_Moons.specialKeywordResult)
                return (RefreshMoonsCataloguePage(ref currentNode, ref loadNode));
            else
                return (true);
        }

        public static bool RefreshMoonsCataloguePage(ref TerminalNode currentNode, ref TerminalNode loadNode)
        {
            //DebugHelper.Log("Running LLL Terminal Event: " + node.terminalEvent + "| EnumValue: " + GetTerminalEventEnum(node.terminalEvent) + " | StringValue: " + GetTerminalEventString(node.terminalEvent));
            if (loadNode.name.Contains("preview") && Enum.TryParse(typeof(PreviewInfoType), GetTerminalEventEnum(loadNode.terminalEvent), out object previewEnumValue))
                Settings.levelPreviewInfoType = (PreviewInfoType)previewEnumValue;
            else if (loadNode.name.Contains("sort") && Enum.TryParse(typeof(SortInfoType), GetTerminalEventEnum(loadNode.terminalEvent), out object sortEnumValue))
                Settings.levelPreviewSortType = (SortInfoType)sortEnumValue;
            else if (loadNode.name.Contains("filter") && Enum.TryParse(typeof(FilterInfoType), GetTerminalEventEnum(loadNode.terminalEvent), out object filterEnumValue))
            {
                Settings.levelPreviewFilterType = (FilterInfoType)filterEnumValue;
                currentTagFilter = GetTerminalEventString(loadNode.terminalEvent);
                //DebugHelper.Log("Tag EventString: " + GetTerminalEventString(loadNode.terminalEvent));
            }

            RefreshExtendedLevelGroups();

            Terminal.modifyingText = true;
            Terminal.screenText.interactable = true;


            Terminal.screenText.text = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);
            Terminal.screenText.textComponent.fontSize = defaultTerminalFontSize - (0.1f * (currentMoonsCataloguePage.ExtendedLevels.Count - OriginalContent.MoonsCatalogue.Count));
            Terminal.currentText = Terminal.TextPostProcess("\n" + "\n" + "\n" + GetMoonsTerminalText(), Terminal.currentNode);

            Terminal.textAdded = 0;

            Terminal.currentNode = Keyword_Moons.specialKeywordResult;
            return (false);
        }

        internal static void FilterMoonsCataloguePage(MoonsCataloguePage moonsCataloguePage)
        {
            List<ExtendedLevel> removeLevelList = new List<ExtendedLevel>();

            foreach (ExtendedLevelGroup extendedLevelGroup in moonsCataloguePage.ExtendedLevelGroups)
                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(extendedLevelGroup.extendedLevelsList))
                {
                    bool removeExtendedLevel = extendedLevel.IsRouteHidden;

                    if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Price))
                        removeExtendedLevel = (extendedLevel.PurchasePrice > Terminal.groupCredits);
                    else if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Weather))
                        removeExtendedLevel = (GetWeatherConditions(extendedLevel) != string.Empty);
                    else if (Settings.levelPreviewFilterType.Equals(FilterInfoType.Tag))
                        removeExtendedLevel = (!extendedLevel.TryGetTag(currentTagFilter));

                    if (removeExtendedLevel == true)
                        removeLevelList.Add(extendedLevel);
                }

            foreach (ExtendedLevelGroup extendedLevelGroup in moonsCataloguePage.ExtendedLevelGroups)
                foreach (ExtendedLevel extendedLevel in removeLevelList)
                    if (extendedLevelGroup.extendedLevelsList.Contains(extendedLevel))
                        extendedLevelGroup.extendedLevelsList.Remove(extendedLevel);

            if (Settings.levelPreviewFilterType != FilterInfoType.None)
                moonsCataloguePage.RebuildLevelGroups(new List<ExtendedLevelGroup>(moonsCataloguePage.ExtendedLevelGroups), Settings.moonsCatalogueSplitCount);
        }

        internal static void SortMoonsCataloguePage(MoonsCataloguePage cataloguePage)
        {
            if (Settings.levelPreviewSortType.Equals(SortInfoType.Price))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.PurchasePrice), Settings.moonsCatalogueSplitCount);
            else if (Settings.levelPreviewSortType.Equals(SortInfoType.Difficulty))
                cataloguePage.RebuildLevelGroups(cataloguePage.ExtendedLevels.OrderBy(o => o.CalculatedDifficultyRating), Settings.moonsCatalogueSplitCount);
        }

        internal static void SetStoryLogAuthorPostProcessText()
        {


        }

        public static void AddTerminalNodeEventListener(TerminalNode node, LoadNodeAction action, LoadNodeActionType loadNodeActionType)
        {
            if (node != null && action != null)
            {
                if (loadNodeActionType == LoadNodeActionType.Before && !onBeforeLoadNewNodeRegisteredEventsDictionary.ContainsKey(node))
                {
                    onBeforeLoadNewNodeRegisteredEventsDictionary.Add(node, action);
                    DebugHelper.Log("Successfully Registered OnBeforeLoadNode Action: " + action.Method.Name + " To TerminalNode: " + node.name, DebugType.Developer);
                }
                else if (loadNodeActionType == LoadNodeActionType.After && !onLoadNewNodeRegisteredEventsDictionary.ContainsKey(node))
                {
                    onLoadNewNodeRegisteredEventsDictionary.Add(node, action);
                    DebugHelper.Log("Successfully Registered OnLoadNode Action: " + action.Method.Name + " To TerminalNode: " + node.name, DebugType.Developer);
                }
            }
        }

        ////////// Getting Data //////////

        internal static string GetMoonsTerminalText()
        {
            string fallbackOverviewText = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company Building   //   Buying at [companyBuyingPercent].\r\n\r\n";
            string overviewText = Keyword_Moons.specialKeywordResult.displayText;
            if (overviewText.Contains("\n\n"))
            {
                overviewText = overviewText.Substring(overviewText.IndexOf("\n\n"));
                overviewText = overviewText.SkipToLetters();
                if (overviewText.Contains("\n\n"))
                {
                    overviewText = overviewText.Substring(overviewText.IndexOf("\n\n"));
                    if (Keyword_Moons.specialKeywordResult.displayText.Contains(overviewText))
                    {
                        overviewText = overviewText.Substring(overviewText.IndexOf("\n\n"));
                        overviewText = Keyword_Moons.specialKeywordResult.displayText.Replace(overviewText, string.Empty) + "\n\n";
                    }
                    else
                    {
                        DebugHelper.LogError("Failed To get Moons Catalogue overview text dynamically, falling back to hardcoded English variant.", DebugType.Developer);
                        overviewText = fallbackOverviewText;
                    }
                }
                else
                {
                    DebugHelper.LogError("Failed To get Moons Catalogue overview text dynamically, falling back to hardcoded English variant.", DebugType.Developer);
                    overviewText = fallbackOverviewText;
                }
            }
            else
            {
                DebugHelper.LogError("Failed To get Moons Catalogue overview text dynamically, falling back to hardcoded English variant.", DebugType.Developer);
                overviewText = fallbackOverviewText;
            }

            return (overviewText + GetMoonCatalogDisplayListings() + "\r\n");
        }

        //This is some absolute super arbitrary wizardry to replicate base game >moons command
        public static string GetMoonCatalogDisplayListings()
        {
            string returnString = string.Empty;

            foreach (ExtendedLevelGroup extendedLevelGroup in currentMoonsCataloguePage.ExtendedLevelGroups)
            {
                string groupString = string.Empty;
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    if (extendedLevel.IsRouteHidden == false)
                        groupString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";
                if (!string.IsNullOrEmpty(groupString))
                    returnString += groupString + "\n";

            }
            if (returnString.Contains("\n"))
                returnString.Replace(returnString.Substring(returnString.LastIndexOf("\n")), "");

            string tagString = Settings.levelPreviewFilterType.ToString().ToUpper();
            if (Settings.levelPreviewFilterType == FilterInfoType.Tag)
                tagString = currentTagFilter.ToUpper();

            return (returnString + "\n" + "____________________________" + "\n" + "PREVIEW: " + Settings.levelPreviewInfoType.ToString().ToUpper() + " | " + "SORT: " + Settings.levelPreviewSortType.ToString().ToUpper() + " | " + "FILTER: " + tagString + "\n");
        }

        public static string GetExtendedLevelPreviewInfo(ExtendedLevel extendedLevel)
        {
            string levelPreviewInfo = string.Empty;
            //string offset = GetOffsetExtendedLevelName(extendedLevel);
            string offset = string.Empty;

            if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Weather))
                levelPreviewInfo = GetWeatherConditions(extendedLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Price))
                levelPreviewInfo = offset + "($" + extendedLevel.PurchasePrice + ")";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Difficulty))
                levelPreviewInfo = offset + "(" + extendedLevel.SelectableLevel.riskLevel + ")";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.History))
                levelPreviewInfo = offset + GetHistoryConditions(extendedLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.All))
                levelPreviewInfo = offset + "(" + extendedLevel.SelectableLevel.riskLevel + ") " + "($" + extendedLevel.PurchasePrice + ") " + GetWeatherConditions(extendedLevel);
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Vanilla))
                levelPreviewInfo = offset + "[planetTime]";
            else if (Settings.levelPreviewInfoType.Equals(PreviewInfoType.Override))
                levelPreviewInfo = offset + Settings.GetOverridePreviewInfo(extendedLevel);
            if (extendedLevel.IsRouteLocked == true)
                levelPreviewInfo += " (Locked)";

            string overridePreviewInfo = onBeforePreviewInfoTextAdded?.Invoke(extendedLevel, Settings.levelPreviewInfoType);
            if (overridePreviewInfo != null && overridePreviewInfo != string.Empty)
                levelPreviewInfo = overridePreviewInfo;

            return (levelPreviewInfo);
        }

        //Just returns the level weather with a space and ().
        public static string GetWeatherConditions(ExtendedLevel extendedLevel)
        {
            string returnString = string.Empty;
            /*if (extendedLevel.currentExtendedWeatherEffect != null)
                returnString = "(" + extendedLevel.currentExtendedWeatherEffect.weatherDisplayName + ")";*/
            if (extendedLevel.SelectableLevel.currentWeather != LevelWeatherType.None)
                returnString = "(" + extendedLevel.SelectableLevel.currentWeather.ToString() + ")";
            return (returnString);
        }

        public static string GetHistoryConditions(ExtendedLevel extendedLevel)
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


        public static string GetSimulationResultsText(ExtendedLevel extendedLevel)
        {
            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = new List<ExtendedDungeonFlowWithRarity>(DungeonManager.GetValidExtendedDungeonFlows(extendedLevel, true).OrderBy(o => -(o.rarity)).ToList());
            string overrideString = "Simulating arrival to " + extendedLevel.SelectableLevel.PlanetName + "\nAnalyzing potential remnants found on surface. \nListing generated probabilities below.\n____________________________ \n\nPOSSIBLE STRUCTURES: \n";
            int totalRarityPool = 0;
            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                totalRarityPool += extendedDungeonFlowResult.rarity;
            foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowResult in availableExtendedFlowsList)
                overrideString += "* " + extendedDungeonFlowResult.extendedDungeonFlow.DungeonName + "  //  Chance: " + GetSimulationDataText(extendedDungeonFlowResult.rarity, totalRarityPool) + "\n";

            return (overrideString);
        }

        public static string GetSimulationDataText(int rarity, int totalRarity)
        {
            string returnString = string.Empty;
            if (Settings.levelSimulateInfoType == SimulateInfoType.Percentage)
                returnString = (((float)rarity / (float)totalRarity * 100).ToString("F2") + "%");
            else if (Settings.levelSimulateInfoType == SimulateInfoType.Rarity)
                returnString = (rarity + " // " + totalRarity);
            return (returnString);
        }

        public static string GetOffsetExtendedLevelName(ExtendedLevel extendedLevel)
        {
            int longestLevelName = 0;
            string returnString = string.Empty;

            foreach (ExtendedLevel currentExtendedLevel in currentMoonsCataloguePage.ExtendedLevels)
            {
                if (currentExtendedLevel.NumberlessPlanetName.Length > longestLevelName)
                    longestLevelName = currentExtendedLevel.NumberlessPlanetName.Length;
            }

            for (int i = 0; i < (longestLevelName - extendedLevel.NumberlessPlanetName.Length); i++)
                returnString += " ";

            return returnString;
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

        public static List<ExtendedLevelGroup> GetExtendedLevelGroups(ExtendedLevel[] newExtendedLevels, int splitCount)
        {
            List<ExtendedLevelGroup> returnList = new List<ExtendedLevelGroup>();

            int counter = 0;
            int levelsAdded = 0;
            List<ExtendedLevel> currentExtendedLevelsBatch = new List<ExtendedLevel>();
            foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevels))
            {
                currentExtendedLevelsBatch.Add(extendedLevel);
                levelsAdded++;
                counter++;

                if (counter == splitCount || levelsAdded == newExtendedLevels.Length)
                {
                    returnList.Add(new ExtendedLevelGroup(currentExtendedLevelsBatch));
                    currentExtendedLevelsBatch.Clear();
                    counter = 0;
                }
            }

            return (returnList);
        }

        ////////// Creating Data //////////

        internal static void CreateExtendedLevelGroups()
        {
            List<ExtendedLevel> hiddenVanillaLevels = new List<ExtendedLevel>();    
            foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaExtendedLevels)
            {
                if (extendedLevel.IsRouteHidden)
                {
                    hiddenVanillaLevels.Add(extendedLevel);
                }
            }

            hiddenVanillaLevels = hiddenVanillaLevels.OrderBy(l => l.CalculatedDifficultyRating).ToList();

            DebugHelper.Log("Creating ExtendedLevelGroups", DebugType.Developer);
            foreach (SelectableLevel level in OriginalContent.MoonsCatalogue)
                DebugHelper.Log(level.PlanetName.ToString(), DebugType.Developer);
            ExtendedLevelGroup vanillaGroupA = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(0, 3));
            ExtendedLevelGroup vanillaGroupB = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(3, 3));
            ExtendedLevelGroup vanillaGroupC = new ExtendedLevelGroup(OriginalContent.MoonsCatalogue.GetRange(6, 3));
            ExtendedLevelGroup vanillaGroupD = new ExtendedLevelGroup(hiddenVanillaLevels);

            Dictionary<string, List<ExtendedLevel>> extendedLevelsContentSourceNameDictionary = new Dictionary<string, List<ExtendedLevel>>();
            foreach (ExtendedLevel customExtendedLevel in PatchedContent.CustomExtendedLevels)
                extendedLevelsContentSourceNameDictionary.AddOrAddAdd(customExtendedLevel.ModName, customExtendedLevel);               

            List<ExtendedLevelGroup> defaultVanillaExtendedLevelGroups = new List<ExtendedLevelGroup>() { vanillaGroupA, vanillaGroupB, vanillaGroupC, vanillaGroupD };
            List<ExtendedLevelGroup> defaultCustomGroupedExtendedLevelGroups = new List<ExtendedLevelGroup>();
            List<ExtendedLevelGroup> defaultCustomSingleExtendedLevelGroups = new List<ExtendedLevelGroup>();

            List<ExtendedLevel> singleExtendedLevelsList = new List<ExtendedLevel>();

            foreach (KeyValuePair<string, List<ExtendedLevel>> customExtendedLevelLists in new Dictionary<string, List<ExtendedLevel>>(extendedLevelsContentSourceNameDictionary))
            {
                extendedLevelsContentSourceNameDictionary[customExtendedLevelLists.Key] = customExtendedLevelLists.Value.OrderBy(o => o.CalculatedDifficultyRating).ToList();
                if (customExtendedLevelLists.Value.Count == 1)
                    singleExtendedLevelsList.Add(customExtendedLevelLists.Value[0]);
                else if (customExtendedLevelLists.Value.Count != 0)
                    foreach (ExtendedLevelGroup extendedLevelGroup in GetExtendedLevelGroups(customExtendedLevelLists.Value.ToArray(), Settings.moonsCatalogueSplitCount))
                        defaultCustomGroupedExtendedLevelGroups.Add(extendedLevelGroup);
            }

            //defaultCustomExtendedLevelGroups.Add(new ExtendedLevelGroup(singleExtendedLevelsList.OrderBy(o => o.CalculatedDifficultyRating).ToList()));
            //defaultCustomExtendedLevelGroups = defaultCustomExtendedLevelGroups.OrderBy(o => o.AverageCalculatedDifficulty).ToList();
            singleExtendedLevelsList = singleExtendedLevelsList.OrderBy(o => o.CalculatedDifficultyRating).ToList();
            defaultCustomSingleExtendedLevelGroups = GetExtendedLevelGroups(singleExtendedLevelsList.ToArray(), Settings.moonsCatalogueSplitCount);

            List<ExtendedLevelGroup> combinedOrderedCustomExtendedLevelGroups = defaultCustomGroupedExtendedLevelGroups.Concat(defaultCustomSingleExtendedLevelGroups).OrderBy(o => o.AverageCalculatedDifficulty).ToList();
            List<ExtendedLevelGroup> allDefaultExtendedLevelGroups = defaultVanillaExtendedLevelGroups.Concat(combinedOrderedCustomExtendedLevelGroups).ToList();
            string debugString = "Debugging DefaultExtendedLevelsGroup" + "\n";
            int counter = 0;
            foreach (ExtendedLevelGroup extendedLevelGroup in allDefaultExtendedLevelGroups)
            {
                debugString += "Group #" + counter + " ";
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    debugString += extendedLevel.NumberlessPlanetName + "(" + extendedLevel.ModName + ") , ";
                debugString += "\n";

                counter++;
            }
            DebugHelper.Log(debugString, DebugType.Developer);
            defaultMoonsCataloguePage = new MoonsCataloguePage(allDefaultExtendedLevelGroups);
            currentMoonsCataloguePage = new MoonsCataloguePage(new List<ExtendedLevelGroup>());
            RefreshExtendedLevelGroups();
        }

        internal static void CreateMoonsFilterTerminalAssets()
        {
            //Preview & Sort Keywords
            foreach (TerminalNode previewNode in CreateTerminalEventNodes("preview", new List<Enum>() { PreviewInfoType.Price, PreviewInfoType.Difficulty, PreviewInfoType.Weather, PreviewInfoType.History, PreviewInfoType.All, PreviewInfoType.None }))
                AddTerminalNodeEventListener(previewNode, TryRefreshMoonsCataloguePage, LoadNodeActionType.Before);

            foreach (TerminalNode sortNode in CreateTerminalEventNodes("sort", new List<Enum>() { SortInfoType.Price, SortInfoType.Difficulty, SortInfoType.None }))
                AddTerminalNodeEventListener(sortNode, TryRefreshMoonsCataloguePage, LoadNodeActionType.Before);

            foreach (TerminalNode filterNode in CreateTerminalEventNodes("filter", new List<Enum>() { FilterInfoType.Price, FilterInfoType.Weather, FilterInfoType.None }))
                AddTerminalNodeEventListener(filterNode, TryRefreshMoonsCataloguePage, LoadNodeActionType.Before);

            //Tag Keywords
            List<string> tagMoonWordsList = new List<string>();
            List<string> tagMoonTerminalEventsList = new List<string>();
            List<string> allLevelTags = new List<string>();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                foreach (ContentTag contentTag in extendedLevel.ContentTags)
                    if (!allLevelTags.Contains(contentTag.contentTagName))
                        allLevelTags.Add(contentTag.contentTagName);
            foreach (string levelTag in allLevelTags)
            {
                tagMoonWordsList.Add(levelTag);
                tagMoonTerminalEventsList.Add("Tag;" + levelTag);
            }

            foreach (TerminalNode filterNode in CreateTerminalEventNodes("filter", tagMoonWordsList, tagMoonTerminalEventsList, createNewVerbKeyword: false))
                AddTerminalNodeEventListener(filterNode, TryRefreshMoonsCataloguePage, LoadNodeActionType.Before);

            //Simulate Keywords
            List<string> simulateMoonsKeywords = new List<string>();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                simulateMoonsKeywords.Add(extendedLevel.TerminalNoun);

            int counter = 0;
            foreach (TerminalNode simulateNode in CreateTerminalEventNodes("simulate", simulateMoonsKeywords))
            {
                AddTerminalNodeEventListener(simulateNode, SetSimulationResultsText, LoadNodeActionType.Before);
                PatchedContent.ExtendedLevels[counter].SimulateNode = simulateNode;
                counter++;
            }
        }

        internal static List<TerminalNode> CreateTerminalEventNodes(string newVerbKeywordWord, List<Enum> terminalEventEnumStrings)
        {
            List<string> convertedList = new List<string>();
            foreach (Enum enumValue in terminalEventEnumStrings)
                convertedList.Add(enumValue.ToString());

            return (CreateTerminalEventNodes(newVerbKeywordWord, convertedList));
        }

        internal static List<TerminalNode> CreateTerminalEventNodes(string newVerbKeywordWord, List<string> nounWords, List<string> terminalEventStrings = null, bool createNewVerbKeyword = true)
        {
            List<TerminalNode> newTerminalNodes = new List<TerminalNode>();
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
                    newTerminalNodes.Add(CreateTerminalEventNode(verbKeyword, newNode, terminalEventStrings[nounWords.IndexOf(newNode)]));

            return (newTerminalNodes);
        }

        internal static TerminalNode CreateTerminalEventNode(TerminalKeyword verbKeyword, string nounWord, string terminalEventString)
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

            return (newNode);
        }

        internal static TerminalKeyword CreateNewTerminalKeyword(string name = null, string word = null, TerminalKeyword defaultVerb = null)
        {
            TerminalKeyword newTerminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            newTerminalKeyword.name = string.IsNullOrEmpty(name) ? "NewLethalLevelLoaderTerminalKeyword" : name;
            newTerminalKeyword.compatibleNouns = new CompatibleNoun[0];
            newTerminalKeyword.word = word == null ? null : word;
            newTerminalKeyword.defaultVerb = defaultVerb;
            Utilities.Insert(ref Terminal.terminalNodes.allKeywords, newTerminalKeyword);
            return (newTerminalKeyword);
        }

        internal static TerminalNode CreateNewTerminalNode(string name = null, string displayText = null)
        {
            TerminalNode newTerminalNode = ScriptableObject.CreateInstance<TerminalNode>();
            newTerminalNode.name = string.IsNullOrEmpty(name) ? "NewLethalLevelLoaderTerminalNode" : name;

            newTerminalNode.displayText = string.IsNullOrEmpty(displayText) ? string.Empty : displayText;
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
#endif