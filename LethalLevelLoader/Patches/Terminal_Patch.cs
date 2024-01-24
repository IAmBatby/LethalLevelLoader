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

        //We cache this because we directly get from it via Index so we don't want other mods changing the list.
        private static List<TerminalKeyword> _cachedAllTerminalKeywordsList;
        internal static List<TerminalKeyword> AllTerminalKeywordsList
        {
            get
            {
                if (Terminal != null && _cachedAllTerminalKeywordsList == null)
                    _cachedAllTerminalKeywordsList = Terminal.terminalNodes.allKeywords.ToList();
                return (_cachedAllTerminalKeywordsList);
            }
        }

        internal static List<ExtendedLevelGroup> currentExtendedLevelGroupsList = new List<ExtendedLevelGroup>();
        internal static List<ExtendedLevelGroup> defaultExtendedLevelGroupsList = new List<ExtendedLevelGroup>();
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

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        //This is also where we add our custom moons into the list
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        internal static void TextPostProcess_Prefix(ref string modifiedDisplayText)
        {
            if (modifiedDisplayText.Contains("Welcome to the exomoons catalogue"))
                modifiedDisplayText = GetMoonsTerminalText();
        }

        internal static string GetMoonsTerminalText()
        {
            string returnString = string.Empty;

            returnString += "\n" + "\n" + "\n" + "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company Building   //   Buying at [companyBuyingPercent].\r\n\r\n";
            returnString += GetMoonCatalogDisplayListings() + "\r\n";

            return (returnString);
        }

        internal static void RefreshMoonsNode()
        {
            DebugHelper.Log("Refreshing Moons Node!");
            List<ExtendedLevelGroup> newExtendedLevelGroupsList = new List<ExtendedLevelGroup>(defaultExtendedLevelGroupsList);

            newExtendedLevelGroupsList = SortExtendedLevelGroups(newExtendedLevelGroupsList);
            newExtendedLevelGroupsList = FilterExtendedLevelGroups(newExtendedLevelGroupsList);

            currentExtendedLevelGroupsList = newExtendedLevelGroupsList;
            if (Terminal.screenText.text.Contains("Welcome to the exomoons catalogue"))
            {
                Terminal.screenText.text = Terminal.TextPostProcess(GetMoonsTerminalText(), Terminal.currentNode);
                Terminal.currentText = Terminal.screenText.text;
            }
        }

        internal static void CreateVanillaExtendedLevelGroups()
        {
            List<ExtendedLevel> moonsCatalogue = new List<ExtendedLevel>();

            foreach (SelectableLevel selectableLevel in SelectableLevel_Patch.prePatchedMoonsCatalogueList)
                moonsCatalogue.Add(SelectableLevel_Patch.GetExtendedLevel(selectableLevel));

            ExtendedLevelGroup vanillaGroupA = new ExtendedLevelGroup(new List<ExtendedLevel>() { moonsCatalogue[0], moonsCatalogue[1], moonsCatalogue[2]});
            ExtendedLevelGroup vanillaGroupB = new ExtendedLevelGroup(new List<ExtendedLevel>() { moonsCatalogue[3], moonsCatalogue[4] });
            ExtendedLevelGroup vanillaGroupC = new ExtendedLevelGroup(new List<ExtendedLevel>() { moonsCatalogue[5], moonsCatalogue[6], moonsCatalogue[7] });

            defaultExtendedLevelGroupsList = new List<ExtendedLevelGroup>() { vanillaGroupA, vanillaGroupB, vanillaGroupC };

            RefreshMoonsNode();
        }

        internal static void CreateCustomExtendedLevelGroups()
        {
            defaultExtendedLevelGroupsList.Add(new ExtendedLevelGroup(SelectableLevel_Patch.customLevelsList));
            RefreshMoonsNode();
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings()
        {
            string returnString = string.Empty;

            int counter = 0;
            foreach (ExtendedLevelGroup extendedLevelGroup in currentExtendedLevelGroupsList)
            {
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";

                counter++;

                if (counter != currentExtendedLevelGroupsList.Count)
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
            DebugHelper.Log("PreviewInfo:" + ModSettings.levelPreviewInfoType.ToString());

            switch (ModSettings.levelPreviewInfoType)
            {
                case PreviewInfoType.Weather:
                    levelPreviewInfo = GetWeatherConditions(extendedLevel.selectableLevel);
                    break;
                case PreviewInfoType.Price:
                    levelPreviewInfo = "(" + extendedLevel.RoutePrice + ")";
                    break;
                case PreviewInfoType.Difficulty:
                    levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ")";
                    break;
                case PreviewInfoType.History:
                    levelPreviewInfo = GetHistoryConditions(extendedLevel);
                    break;
                case PreviewInfoType.All:
                    levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ") " + "(" + extendedLevel.RoutePrice + ") " + GetWeatherConditions(extendedLevel.selectableLevel);
                    break;
                case PreviewInfoType.None:
                    break;
                case PreviewInfoType.Vanilla:
                    levelPreviewInfo = "[planetTime]";
                    break;
                case PreviewInfoType.Override:
                    levelPreviewInfo = ModSettings.GetOverridePreviewInfo(extendedLevel);
                    break;
                default:
                    break;
            }

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
            TerminalKeyword tempRouteKeyword = GetTerminalKeywordFromIndex(26);
            TerminalKeyword tempInfoKeyword = GetTerminalKeywordFromIndex(6);


            TerminalKeyword terminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            TerminalNode terminalNodeRoute = ScriptableObject.CreateInstance<TerminalNode>();
            TerminalNode terminalNodeRouteConfirm = ScriptableObject.CreateInstance<TerminalNode>();
            TerminalNode terminalNodeInfo = ScriptableObject.CreateInstance<TerminalNode>();

            terminalKeyword.compatibleNouns = new CompatibleNoun[0];
            terminalKeyword.name = extendedLevel.NumberlessPlanetName;
            terminalKeyword.word = extendedLevel.NumberlessPlanetName.ToLower();
            terminalKeyword.defaultVerb = tempRouteKeyword;


            terminalNodeRoute.name = extendedLevel.NumberlessPlanetName.ToLower() + "Route";
            terminalNodeRoute.displayText = "The cost to route to " + extendedLevel.selectableLevel.PlanetName + " is [totalCost]. It is currently [currentPlanetTime] on this moon.";
            terminalNodeRoute.displayText += "\n" + "\n" + "Please CONFIRM or DENY." + "\n" + "\n";
            terminalNodeRoute.clearPreviousText = true;
            terminalNodeRoute.maxCharactersToType = 25;
            terminalNodeRoute.buyItemIndex = -1;
            terminalNodeRoute.buyRerouteToMoon = -2;
            terminalNodeRoute.displayPlanetInfo = extendedLevel.selectableLevel.levelID;
            terminalNodeRoute.shipUnlockableID = -1;
            terminalNodeRoute.itemCost = routePrice;
            terminalNodeRoute.creatureFileID = -1;
            terminalNodeRoute.storyLogFileID = -1;
            terminalNodeRoute.overrideOptions = true;
            terminalNodeRoute.playSyncedClip = -1;

            terminalNodeRouteConfirm.terminalOptions = new CompatibleNoun[0];
            terminalNodeRouteConfirm.name = extendedLevel.NumberlessPlanetName.ToLower() + "RouteConfirm";
            terminalNodeRouteConfirm.displayText = "Routing autopilot to " + extendedLevel.selectableLevel.PlanetName + " Your new balance is [playerCredits].";
            terminalNodeRouteConfirm.clearPreviousText = true;
            terminalNodeRouteConfirm.maxCharactersToType = 25;
            terminalNodeRouteConfirm.buyItemIndex = -1;
            terminalNodeRouteConfirm.buyRerouteToMoon = extendedLevel.selectableLevel.levelID;
            terminalNodeRouteConfirm.displayPlanetInfo = 1;
            terminalNodeRouteConfirm.shipUnlockableID = -1;
            terminalNodeRouteConfirm.itemCost = routePrice;
            terminalNodeRouteConfirm.creatureFileID = -1;
            terminalNodeRouteConfirm.storyLogFileID = -1;
            terminalNodeRouteConfirm.overrideOptions = true;
            terminalNodeRouteConfirm.playSyncedClip = -1;


            //Building The TerminalNodeInfo String

            string infoString = string.Empty;

            infoString += extendedLevel.selectableLevel.PlanetName + "\n";
            infoString += "----------------------" + "\n" + "\n";
            int subStringIndex = extendedLevel.selectableLevel.LevelDescription.IndexOf("CONDITIONS");
            string modifiedDescription = extendedLevel.selectableLevel.LevelDescription.Substring(subStringIndex);
            subStringIndex = modifiedDescription.IndexOf("FAUNA");
            modifiedDescription = modifiedDescription.Insert(subStringIndex, "\n");

            infoString += modifiedDescription;

            //

            terminalNodeRouteConfirm.terminalOptions = new CompatibleNoun[0];
            terminalNodeInfo.name = extendedLevel.NumberlessPlanetName.ToLower() + "Info";
            terminalNodeInfo.displayText = infoString;
            terminalNodeInfo.clearPreviousText = true;
            terminalNodeInfo.maxCharactersToType = 25;
            terminalNodeInfo.buyItemIndex = -1;
            terminalNodeInfo.buyRerouteToMoon = -1;
            terminalNodeInfo.displayPlanetInfo = -1;
            terminalNodeInfo.shipUnlockableID = -1;
            terminalNodeInfo.itemCost = 0;
            terminalNodeInfo.creatureFileID = -1;
            terminalNodeInfo.storyLogFileID = 1;
            terminalNodeInfo.playSyncedClip = -1;

            CompatibleNoun routeDeny = new CompatibleNoun();
            CompatibleNoun routeConfirm = new CompatibleNoun();

            routeDeny.noun = DenyKeyword;
            routeDeny.result = CancelRouteNode;

            routeConfirm.noun = ConfirmKeyword;
            routeConfirm.result = terminalNodeRouteConfirm;

            terminalNodeRoute.terminalOptions = terminalNodeRoute.terminalOptions.AddItem(routeDeny).ToArray();
            terminalNodeRoute.terminalOptions = terminalNodeRoute.terminalOptions.AddItem(routeConfirm).ToArray();

            CompatibleNoun routeLevel = new CompatibleNoun();

            routeLevel.noun = terminalKeyword;
            routeLevel.result = terminalNodeRoute;

            CompatibleNoun infoLevel = new CompatibleNoun();

            infoLevel.noun = terminalKeyword;
            infoLevel.result = terminalNodeInfo;

            tempInfoKeyword.compatibleNouns = tempInfoKeyword.compatibleNouns.AddItem(infoLevel).ToArray();

            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(terminalKeyword).ToArray();
            tempRouteKeyword.compatibleNouns = tempRouteKeyword.compatibleNouns.AddItem(routeLevel).ToArray();

            newRouteNode = terminalNodeRoute;
        }

        internal static void CreateSimulateTravelTerminalAssets()
        {

        }

        internal static void CreateMoonsFilterTerminalAssets()
        {
            previewKeywordsGroup = new DynamicKeywordGroup("preview", new List<Enum>() { PreviewInfoType.Price, PreviewInfoType.Difficulty, PreviewInfoType.Weather, PreviewInfoType.History, PreviewInfoType.All, PreviewInfoType.None });
            sortKeywordGroup = new DynamicKeywordGroup("sort", new List<Enum>() { SortInfoType.Price, SortInfoType.Difficulty, SortInfoType.None });

            List<StringWithEnum> tagStringWithEnumsList = new List<StringWithEnum>();
            List<string> allUniqueLevelTags = new List<string>();
            foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.allLevelsList)
                foreach (string levelTag in extendedLevel.levelTags)
                    if (!allUniqueLevelTags.Contains(levelTag))
                        allUniqueLevelTags.Add(levelTag);

            foreach (string levelTag in allUniqueLevelTags)
                tagStringWithEnumsList.Add(new StringWithEnum(FilterInfoType.Tag, levelTag));

            tagStringWithEnumsList.Add(new StringWithEnum(FilterInfoType.Price));
            tagStringWithEnumsList.Add(new StringWithEnum(FilterInfoType.Weather));
            tagStringWithEnumsList.Add(new StringWithEnum(FilterInfoType.None));


            filterKeywordGroup = new DynamicKeywordGroup("filter", tagStringWithEnumsList);

            List<StringWithEnum> simulateMoonsKeywords = new List<StringWithEnum>();

            foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.allLevelsList)
                simulateMoonsKeywords.Add(new StringWithEnum(extendedLevel.levelType, extendedLevel.NumberlessPlanetName));

            new DynamicKeywordGroup("simulate", simulateMoonsKeywords);
        }

        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static bool RunTerminalEvents(TerminalNode node)
        {
            bool requiresRefresh = false;

            if (node != null && string.IsNullOrEmpty(node.terminalEvent) == false)
            {
                DebugHelper.Log("Running LLL Terminal Event: " + node.terminalEvent);

                currentTagFilter = string.Empty;

                
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
                    ModSettings.levelPreviewFilterType = (FilterInfoType)filterStringWithEnum.enumValue;
                    currentTagFilter = filterStringWithEnum.stringValue;
                    requiresRefresh = true;
                }
                else
                {
                    foreach (ExtendedLevel extendedLevel in SelectableLevel_Patch.allLevelsList)
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
                    RefreshMoonsNode();
                    return (false);
                }
            }

            return (true);
        }

        /*internal static void TogglePreviewInfo(PreviewInfoType previewInfoTypeToggle)
        {
            ModSettings.levelPreviewInfoType = previewInfoTypeToggle;
            RefreshMoonsNode(currentExtendedLevelGroupsList);
        }*/

        /*internal static void ToggleSortInfo(SortInfoType sortInfoTypeToggle)
        {
            List<ExtendedLevelGroup> newExtendedLevelGroupsList = null;
            ModSettings.levelPreviewSortType = sortInfoTypeToggle;

            DebugHelper.Log("Sorting Levels As: " + ModSettings.levelPreviewSortType.ToString());   

            if (ModSettings.levelPreviewSortType == SortInfoType.None)
            {
                newExtendedLevelGroupsList = new List<ExtendedLevelGroup>(defaultExtendedLevelGroupsList);
            }
            else if (ModSettings.levelPreviewSortType == SortInfoType.Price)
            {
                DebugHelper.DebugExtendedLevelGroups(currentExtendedLevelGroupsList);
                Debug.Log("1 " + currentExtendedLevelGroupsList.Count);
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(currentExtendedLevelGroupsList)).OrderBy(o => o.RoutePrice).ToList();
                Debug.Log("2 " + newExtendedLevelsList.Count);
                newExtendedLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
            }
            else if (ModSettings.levelPreviewSortType == SortInfoType.Difficulty)
            {
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(currentExtendedLevelGroupsList)).OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount).ToList();
                newExtendedLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
            }
            Debug.Log("3");
            DebugHelper.DebugExtendedLevelGroups(newExtendedLevelGroupsList);
            RefreshMoonsNode(new List<ExtendedLevelGroup>(newExtendedLevelGroupsList));
        }*/

        /*internal static void ToggleFilterInfo(StringWithEnum stringWithEnum)
        {
            string tag = stringWithEnum.stringValue;
            DebugHelper.Log("Filtering Moons Via Tag: " + tag);
            ModSettings.levelPreviewFilterType = (FilterInfoType)stringWithEnum.enumValue;

            if (ModSettings.levelPreviewFilterType == FilterInfoType.Price)
            {
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(defaultExtendedLevelGroupsList));

                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                    if (extendedLevel.RoutePrice > Terminal.groupCredits)
                        newExtendedLevelsList.Remove(extendedLevel);

                RefreshMoonsNode(CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3));
            }
            else if (ModSettings.levelPreviewFilterType == FilterInfoType.Tag)
            {
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(defaultExtendedLevelGroupsList));

                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                    if (!extendedLevel.levelTags.Contains(tag))
                        newExtendedLevelsList.Remove(extendedLevel);

                RefreshMoonsNode(CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3));
            }
            else if (ModSettings.levelPreviewFilterType == FilterInfoType.None)
            {
                currentExtendedLevelGroupsList = new List<ExtendedLevelGroup>(defaultExtendedLevelGroupsList);
                ToggleSortInfo(ModSettings.levelPreviewSortType);
            }
        }*/

        internal static List<ExtendedLevelGroup> FilterExtendedLevelGroups(List<ExtendedLevelGroup> extendedLevelGroups)
        {
            List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(extendedLevelGroups));
            List<ExtendedLevelGroup> filteredLevelGroupsList = new List<ExtendedLevelGroup>();

            switch (ModSettings.levelPreviewFilterType)
            {
                case FilterInfoType.Price:
                    foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                        if (extendedLevel.RoutePrice > Terminal.groupCredits)
                            newExtendedLevelsList.Remove(extendedLevel);

                    filteredLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
                    break;

                case FilterInfoType.Weather:
                    foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                        if (GetWeatherConditions(extendedLevel.selectableLevel) != string.Empty)
                            newExtendedLevelsList.Remove(extendedLevel);

                    filteredLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
                    break;

                case FilterInfoType.Tag:
                    foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                        if (!extendedLevel.levelTags.Contains(currentTagFilter))
                            newExtendedLevelsList.Remove(extendedLevel);

                    filteredLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
                    break;

                case FilterInfoType.TraveledThisQuota:
                    break;
                case FilterInfoType.TraveledThisRun:
                    break;
                case FilterInfoType.None:
                    filteredLevelGroupsList = extendedLevelGroups;
                    break;
            }

            return (filteredLevelGroupsList);
        }

        internal static List<ExtendedLevelGroup> SortExtendedLevelGroups(List<ExtendedLevelGroup> extendedLevelGroups)
        {
            List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(extendedLevelGroups));
            List<ExtendedLevelGroup> filteredLevelGroupsList = new List<ExtendedLevelGroup>();

            switch (ModSettings.levelPreviewSortType)
            {
                case SortInfoType.Price:
                    newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(extendedLevelGroups)).OrderBy(o => o.RoutePrice).ToList();
                    filteredLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
                    break;
                case SortInfoType.Difficulty:
                    newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(extendedLevelGroups)).OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount).ToList();
                    filteredLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
                    break;
                case SortInfoType.Tag:
                    break;
                case SortInfoType.LastTraveled:
                    break;
                case SortInfoType.None:
                    filteredLevelGroupsList = extendedLevelGroups;
                    break;
            }

            return (filteredLevelGroupsList);
        }

        internal static List<ExtendedLevelGroup> CreateExtendedLevelGroups(List<ExtendedLevel> extendedLevelsList, int splitCount)
        {
            List<ExtendedLevelGroup> returnList = new List<ExtendedLevelGroup>();

            int counter = 0;
            int levelsAdded = 0;
            List<ExtendedLevel> currentExtendedLevelsBatch = new List<ExtendedLevel>();
            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
            {
                currentExtendedLevelsBatch.Add(extendedLevel);
                levelsAdded++;
                counter++;

                if (counter == splitCount || levelsAdded == extendedLevelsList.Count)
                {
                    Debug.Log("Spltting!");
                    returnList.Add(new ExtendedLevelGroup(currentExtendedLevelsBatch));
                    currentExtendedLevelsBatch.Clear();
                    counter = 0;
                }
            }

            return (returnList);
        }

        public static List<ExtendedLevel> GetExtendedLevels(List<ExtendedLevelGroup> extendedLevelGroups)
        {
            List<ExtendedLevel> returnList = new List<ExtendedLevel>();

            foreach (ExtendedLevelGroup extendedLevelGroup in extendedLevelGroups)
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    returnList.Add(extendedLevel);

            return returnList;
        }

        internal static (TerminalKeyword, TerminalNode) CreateTerminalEventNode(TerminalKeyword verbKeyword, string nounWord, string eventString = null)
        {
            DebugHelper.Log("Creating New TerminalEvent Node! VerbKeyword Word Is: " + verbKeyword.word + " | nounWord Is: " + nounWord + " | EventString Is: " + eventString);
            TerminalKeyword newKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            TerminalNode newNode = ScriptableObject.CreateInstance<TerminalNode>();

            if (eventString == null)
                eventString = nounWord;


            newKeyword.word = nounWord.ToLower();
            newKeyword.defaultVerb = verbKeyword;
            newNode.displayText = string.Empty;
            newNode.terminalEvent = eventString;
            newNode.clearPreviousText = false;
            newNode.maxCharactersToType = 25;
            newNode.buyItemIndex = -1;
            newNode.buyRerouteToMoon = -1;
            newNode.displayPlanetInfo = -1;
            newNode.lockedInDemo = false;
            newNode.shipUnlockableID = -1;
            newNode.itemCost = 0;
            newNode.creatureFileID = -1;
            newNode.storyLogFileID = -1;
            newNode.playSyncedClip = -1;

            CompatibleNoun newCompatibleNoun = new CompatibleNoun();
            newCompatibleNoun.noun = newKeyword;
            newCompatibleNoun.result = newNode;

            verbKeyword.compatibleNouns = verbKeyword.compatibleNouns.AddItem(newCompatibleNoun).ToArray();
            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(newKeyword).ToArray();

            return (newKeyword, newNode);
        }

        internal static TerminalKeyword GetTerminalKeywordFromIndex(int index)
        {
            if (Terminal != null)
                return (AllTerminalKeywordsList[index]);
            else
                return (null);
        }
    }
}