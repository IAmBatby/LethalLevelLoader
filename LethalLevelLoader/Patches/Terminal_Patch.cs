using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace LethalLevelLoader
{
    public class DynamicKeywordGroup
    {
        private string verbKeywordWord;
        private List<string> nounKeywordWords = new List<string>();

        public TerminalKeyword verbKeyword;
        public List<(TerminalNode eventNode, int enumIndex)> eventNodesList = new List<(TerminalNode eventNode, int enumIndex)>();
        public List<(TerminalKeyword, TerminalNode)> nounKeywordNodePairs = new List<(TerminalKeyword, TerminalNode)>();

        public DynamicKeywordGroup(string newVerbKeywordWord, List<string> newNounKeywordWords)
        {
            verbKeywordWord = newVerbKeywordWord;
            nounKeywordWords = newNounKeywordWords;

            TerminalKeyword newVerbKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            newVerbKeyword.word = verbKeywordWord;
            newVerbKeyword.isVerb = true;

            foreach (string nounKeywordWord in nounKeywordWords)
                nounKeywordNodePairs.Add(Terminal_Patch.CreateTerminalEventNode(newVerbKeyword, nounKeywordWord));

            Terminal_Patch.Terminal.terminalNodes.allKeywords = Terminal_Patch.Terminal.terminalNodes.allKeywords.AddItem(newVerbKeyword).ToArray();
            verbKeyword = newVerbKeyword;
        }

        public bool IsDynamicKeywordsEvent(string terminalNodeEvent)
        {
            foreach ((TerminalKeyword, TerminalNode) nounKeywordNodePairs in nounKeywordNodePairs)
                if (nounKeywordNodePairs.Item2.terminalEvent.Contains(terminalNodeEvent))
                    return (true);

            return (false);
        }

        public bool IsDynamicKeywordsEvent(string terminalNodeEvent, out (TerminalKeyword, TerminalNode) terminalEventNode)
        {
            terminalEventNode = (null, null);

            foreach ((TerminalKeyword, TerminalNode) nounKeywordNodePair in nounKeywordNodePairs)
                if (nounKeywordNodePair.Item2.terminalEvent.Contains(terminalNodeEvent))
                    terminalEventNode = nounKeywordNodePair;

            return (terminalEventNode.Item1 != null && terminalEventNode.Item2 != null);
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

        internal static DynamicKeywordGroup toggleKeywordGroup;
        internal static DynamicKeywordGroup sortKeywordGroup;
        internal static DynamicKeywordGroup filterKeywordGroup;

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

        internal static void RefreshMoonsNode(List<ExtendedLevelGroup> extendedLevelGroups)
        {
            DebugHelper.Log("Refreshing Moons Node!");
            currentExtendedLevelGroupsList = extendedLevelGroups;
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

            defaultExtendedLevelGroupsList.Clear();
            defaultExtendedLevelGroupsList.Add(vanillaGroupA);
            defaultExtendedLevelGroupsList.Add(vanillaGroupB);
            defaultExtendedLevelGroupsList.Add(vanillaGroupC);

            RefreshMoonsNode(defaultExtendedLevelGroupsList);
        }

        internal static void CreateCustomExtendedLevelGroups()
        {
            defaultExtendedLevelGroupsList.Add(new ExtendedLevelGroup(SelectableLevel_Patch.customLevelsList));
            RefreshMoonsNode(defaultExtendedLevelGroupsList);
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

            if (ModSettings.levelPreviewInfoType != LevelPreviewInfoToggleType.None)
                returnString += "\n" + "____________________________" + "\n" + "PREVIEW: " + ModSettings.levelPreviewInfoType.ToString().ToUpper() + " | " + "SORT: " + ModSettings.levelPreviewSortType.ToString().ToUpper() + " | " + "FILTER: " + ModSettings.levelPreviewFilterType.ToString().ToUpper() + "\n";

            return (returnString);
        }

        internal static string GetExtendedLevelPreviewInfo(ExtendedLevel extendedLevel)
        {
            string levelPreviewInfo = string.Empty;
            DebugHelper.Log("PreviewInfo:" + ModSettings.levelPreviewInfoType.ToString());

            switch (ModSettings.levelPreviewInfoType)
            {
                case LevelPreviewInfoToggleType.Weather:
                    levelPreviewInfo = GetWeatherConditions(extendedLevel.selectableLevel);
                    break;
                case LevelPreviewInfoToggleType.Price:
                    levelPreviewInfo = "(" + extendedLevel.RoutePrice + ")";
                    break;
                case LevelPreviewInfoToggleType.Difficulty:
                    levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ")";
                    break;
                case LevelPreviewInfoToggleType.None:
                    break;
                case LevelPreviewInfoToggleType.Vanilla:
                    levelPreviewInfo = "[planetTime]";
                    break;
                case LevelPreviewInfoToggleType.Override:
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
            terminalNodeInfo.maxCharactersToType = 35;
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

        internal static void CreateMoonsFilterTerminalAssets()
        {
            string[] toggleOptions = new string[] { "Price", "Difficulty", "Weather", "None" };
            toggleKeywordGroup = new DynamicKeywordGroup("preview", toggleOptions.ToList());

            string[] sortOptions = new string[] { "Price", "Difficulty", "Test3", "Test4", "Clear" };
            sortKeywordGroup = new DynamicKeywordGroup("sort", sortOptions.ToList());

            string[] filterOptions = new string[] { "Price", "Snow", "Custom", "Clear" };
            filterKeywordGroup = new DynamicKeywordGroup("filter", filterOptions.ToList());
        }


        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        internal static void RunTerminalEvents(TerminalNode node)
        {
            if (node != null && string.IsNullOrEmpty(node.terminalEvent) == false)
            {
                if (toggleKeywordGroup.IsDynamicKeywordsEvent(node.terminalEvent, out (TerminalKeyword, TerminalNode) toggleTerminalEventNodePair))
                    TogglePreviewInfo(toggleTerminalEventNodePair);

                if (sortKeywordGroup.IsDynamicKeywordsEvent(node.terminalEvent, out (TerminalKeyword, TerminalNode) sortTerminalEventNodePair))
                    SortPreviewInfo(sortTerminalEventNodePair);

                if (filterKeywordGroup.IsDynamicKeywordsEvent(node.terminalEvent, out (TerminalKeyword, TerminalNode) filterTerminalEventNodePair))
                    FilterPreviewInfo(filterTerminalEventNodePair);
            }
        }

        internal static void TogglePreviewInfo((TerminalKeyword,TerminalNode) terminalEventNodePair)
        {
            ModSettings.levelPreviewInfoType = (LevelPreviewInfoToggleType)toggleKeywordGroup.nounKeywordNodePairs.IndexOf(terminalEventNodePair);
            RefreshMoonsNode(currentExtendedLevelGroupsList);
        }

        internal static void SortPreviewInfo((TerminalKeyword, TerminalNode) terminalEventNodePair)
        {
            List<ExtendedLevelGroup> newExtendedLevelGroupsList = null;
            ModSettings.levelPreviewSortType = (LevelPreviewInfoSortType)sortKeywordGroup.nounKeywordNodePairs.IndexOf(terminalEventNodePair);

            DebugHelper.Log("Sorting Levels As: " + ModSettings.levelPreviewSortType.ToString());   

            if (ModSettings.levelPreviewSortType == LevelPreviewInfoSortType.None)
            {
                newExtendedLevelGroupsList = new List<ExtendedLevelGroup>(defaultExtendedLevelGroupsList);
            }
            else if (ModSettings.levelPreviewSortType == LevelPreviewInfoSortType.Price)
            {
                DebugHelper.DebugExtendedLevelGroups(currentExtendedLevelGroupsList);
                Debug.Log("1 " + currentExtendedLevelGroupsList.Count);
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(currentExtendedLevelGroupsList)).OrderBy(o => o.RoutePrice).ToList();
                Debug.Log("2 " + newExtendedLevelsList.Count);
                newExtendedLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
            }
            else if (ModSettings.levelPreviewSortType == LevelPreviewInfoSortType.Difficulty)
            {
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(currentExtendedLevelGroupsList)).OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount).ToList();
                newExtendedLevelGroupsList = CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3);
            }
            Debug.Log("3");
            DebugHelper.DebugExtendedLevelGroups(newExtendedLevelGroupsList);
            RefreshMoonsNode(new List<ExtendedLevelGroup>(newExtendedLevelGroupsList));
        }

        internal static void FilterPreviewInfo((TerminalKeyword, TerminalNode) terminalEventNodePair)
        {
            ModSettings.levelPreviewFilterType = (LevelPreviewInfoFilterType)filterKeywordGroup.nounKeywordNodePairs.IndexOf(terminalEventNodePair);

            if (ModSettings.levelPreviewFilterType == LevelPreviewInfoFilterType.Price)
            {
                List<ExtendedLevel> newExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(currentExtendedLevelGroupsList));

                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(newExtendedLevelsList))
                    if (extendedLevel.RoutePrice > Terminal.groupCredits)
                        newExtendedLevelsList.Remove(extendedLevel);

                RefreshMoonsNode(CreateExtendedLevelGroups(newExtendedLevelsList, splitCount: 3));
            }
        }

        internal static List<ExtendedLevelGroup> CreateExtendedLevelGroups(List<ExtendedLevel> extendedLevelsList, int splitCount)
        {
            DebugHelper.Log("During Call");
            List<ExtendedLevelGroup> returnList = new List<ExtendedLevelGroup>();

            DebugHelper.Log("Creating New ExtendedLevelGroups");

            int counter1 = 1;
            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
            {
                DebugHelper.Log("Level " + counter1 + " /" + extendedLevelsList.Count + " : " + extendedLevel.NumberlessPlanetName);
                counter1++;
            }

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

            Debug.Log(returnList.Count);

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

        internal static (TerminalKeyword, TerminalNode) CreateTerminalEventNode(TerminalKeyword verbKeyword, string eventString)
        {
            TerminalKeyword newKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            TerminalNode newNode = ScriptableObject.CreateInstance<TerminalNode>();

            newKeyword.word = eventString.ToLower();
            newKeyword.defaultVerb = verbKeyword;
            newNode.displayText = string.Empty;
            newNode.terminalEvent = verbKeyword.word + eventString;
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