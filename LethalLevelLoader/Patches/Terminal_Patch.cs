using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace LethalLevelLoader
{
    public class Terminal_Patch
    {
        private static Terminal _terminal;
        internal static Terminal Terminal
        {
            get
            {
                if (_terminal != null)
                    return (_terminal);
                else
                {
                    _terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
                    if (_terminal != null)
                        return (_terminal);
                    else
                    {
                        Debug.LogError("LethaLib: Failed To Grab Terminal Reference!");
                        return (null);
                    }
                }

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
        internal static List<ExtendedLevelGroup> defaultExtendedLevelsGroupList = new List<ExtendedLevelGroup>();
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

            returnString += "\n" + "\n" + "\n" + "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company building   //   Buying at [companyBuyingPercent].\r\n\r\n";
            returnString += GetMoonCatalogDisplayListings(Terminal_Patch.Terminal.moonsCatalogueList.ToList()) + "\r\n";

            return (returnString);
        }

        internal static void RefreshMoonsListing(List<ExtendedLevelGroup> extendedLevelGroups)
        {
            currentExtendedLevelGroupsList = extendedLevelGroups;
            RefreshMoonsNode();
        }

        internal static void RefreshMoonsNode()
        {
            //float cachedScrollbarValue = Terminal.scrollBarVertical.value;
            //float cachedScrollbarSize = Terminal.scrollBarVertical.size;
            //Terminal.LoadNewNode(MoonsKeyword.specialKeywordResult);
            //Terminal.scrollBarVertical.value = cachedScrollbarValue;
            //Terminal.scrollBarVertical.size = cachedScrollbarSize;

            if (Terminal.screenText.text.Contains("Welcome to the exomoons catalogue"))
            {
                Terminal.screenText.text = Terminal.TextPostProcess(GetMoonsTerminalText(), Terminal.currentNode);
                Terminal.currentText = Terminal.screenText.text;
            }
        }

        internal static void CreateVanillaExtendedLevelGroups()
        {
            ExtendedLevelGroup vanillaGroupA = new ExtendedLevelGroup();
            vanillaGroupA.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[0]));
            vanillaGroupA.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[1]));
            vanillaGroupA.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[2]));
            defaultExtendedLevelsGroupList.Add(vanillaGroupA);

            ExtendedLevelGroup vanillaGroupB = new ExtendedLevelGroup();
            vanillaGroupB.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[3]));
            vanillaGroupB.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[4]));
            defaultExtendedLevelsGroupList.Add(vanillaGroupB);

            ExtendedLevelGroup vanillaGroupC = new ExtendedLevelGroup();
            vanillaGroupC.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[5]));
            vanillaGroupC.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[6]));
            vanillaGroupC.extendedLevelsList.Add(SelectableLevel_Patch.GetExtendedLevel(SelectableLevel_Patch.prePatchedMoonsCatalogueList[7]));
            defaultExtendedLevelsGroupList.Add(vanillaGroupC);

            RefreshMoonsListing(defaultExtendedLevelsGroupList);
        }

        internal static void CreateCustomExtendedLevelGroups()
        {
            ExtendedLevelGroup customGroup = new ExtendedLevelGroup();

            foreach (ExtendedLevel customLevel in SelectableLevel_Patch.customLevelsList)
                customGroup.extendedLevelsList.Add(customLevel);

            defaultExtendedLevelsGroupList.Add(customGroup);

            RefreshMoonsListing(defaultExtendedLevelsGroupList);
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings(List<SelectableLevel> selectableLevels)
        {
            string returnString = string.Empty;

            foreach (ExtendedLevelGroup extendedLevelGroup in currentExtendedLevelGroupsList)
            {
                foreach (ExtendedLevel extendedLevel in extendedLevelGroup.extendedLevelsList)
                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";
                returnString += "\n";
            }

            return (returnString);
        }

        internal static string GetExtendedLevelPreviewInfo(ExtendedLevel extendedLevel)
        {
            string levelPreviewInfo = string.Empty;

            switch (LethalLevelLoaderSettings.levelPreviewInfoType)
            {
                case LevelPreviewInfoType.Weather:
                    levelPreviewInfo = GetWeatherConditions(extendedLevel.selectableLevel);
                    break;
                case LevelPreviewInfoType.Price:
                    levelPreviewInfo = "(" + extendedLevel.routePrice + ")";
                    break;
                case LevelPreviewInfoType.Difficulty:
                    levelPreviewInfo = "(" + extendedLevel.selectableLevel.riskLevel + ")";
                    break;
                case LevelPreviewInfoType.Empty:
                    break;
                case LevelPreviewInfoType.Vanilla:
                    levelPreviewInfo = "[planetTime]";
                    break;
                case LevelPreviewInfoType.Override:
                    levelPreviewInfo = LethalLevelLoaderSettings.GetOverridePreviewInfo(extendedLevel);
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

        internal static void CreateLevelTerminalData(ExtendedLevel extendedLevel)
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
            terminalNodeRoute.itemCost = extendedLevel.routePrice;
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
            terminalNodeRouteConfirm.itemCost = extendedLevel.routePrice;
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
        }

        internal static void CreateMoonsFilterTerminalAssets()
        {
            string[] toggleOptions = new string[] { "Weather", "Price", "Difficulty", "None" };
            TerminalKeyword toggleKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            toggleKeyword.word = "toggle";
            toggleKeyword.isVerb = true;

            foreach (string filterOption in toggleOptions)
                CreateTerminalEventNode(toggleKeyword, filterOption);

            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(toggleKeyword).ToArray();

            //////////
            
            string[] sortOptions = new string[] { "Price", "Difficulty", "Test3", "Test4", "Clear" };
            TerminalKeyword sortKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            sortKeyword.word = "sort";
            sortKeyword.isVerb = true;

            foreach (string sortOption in sortOptions)
                CreateTerminalEventNode(sortKeyword, sortOption);

            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(sortKeyword).ToArray();
        }


        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        internal static void RunTerminalEvents(TerminalNode node)
        {
            DebugHelper.Log("Running TerminalEvent With TerminalEventString: " + node.terminalEvent);
            List<string> toggleOptions = new List<string> { "Weather", "Price", "Difficulty", "None" };
            List<string> sortOptions = new List<string> { "Price", "Difficulty", "Test3", "Test4", "Clear" };
            List<string> filterOptions = new List<string> {  "Price"}

            if (node.terminalEvent.Contains("toggle"))
                foreach (string toggleString in toggleOptions)
                    if (node.terminalEvent.Contains(toggleString))
                        TogglePreviewInfo(toggleOptions.IndexOf(toggleString));

            if (node.terminalEvent.Contains("sort"))
                foreach (string sortString in sortOptions)
                    if (node.terminalEvent.Contains(sortString))
                        ToggleSortedExtendedLevelGroups(sortString);
        }

        internal static void TogglePreviewInfo(int typeIndex)
        {
            LethalLevelLoaderSettings.levelPreviewInfoType = (LevelPreviewInfoType)typeIndex;
            RefreshMoonsNode();
        }

        internal static void ToggleSortedExtendedLevelGroups(string input)
        {
            if (input == "Clear")
            {
                RefreshMoonsListing(defaultExtendedLevelsGroupList);
            }
            else if (input == "Price")
            {
                List<ExtendedLevel> currentExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(defaultExtendedLevelsGroupList)).OrderBy(o => o.routePrice).ToList();

                RefreshMoonsListing(CreateExtendedLevelGroups(currentExtendedLevelsList, splitCount: 3));
            }
            else if (input == "Difficulty")
            {
                List<ExtendedLevel> currentExtendedLevelsList = new List<ExtendedLevel>(GetExtendedLevels(defaultExtendedLevelsGroupList)).OrderBy(o => o.selectableLevel.maxScrap * o.selectableLevel.maxEnemyPowerCount).ToList();

                RefreshMoonsListing(CreateExtendedLevelGroups(currentExtendedLevelsList, splitCount: 3));
            }
        }

        internal static List<ExtendedLevelGroup> CreateExtendedLevelGroups(List<ExtendedLevel> extendedLevelsList, int splitCount)
        {
            List<ExtendedLevelGroup> returnList = new List<ExtendedLevelGroup>();

            int counter = 0;
            int levelsAdded = 0;
            ExtendedLevelGroup currentGroup = new ExtendedLevelGroup();
            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
            {
                currentGroup.extendedLevelsList.Add(extendedLevel);
                levelsAdded++;
                counter++;

                if (counter == splitCount || levelsAdded == extendedLevelsList.Count)
                {
                    Debug.Log("Spltting!");
                    ExtendedLevelGroup returnLevelGroup = new ExtendedLevelGroup();
                    returnLevelGroup.extendedLevelsList = new List<ExtendedLevel>(currentGroup.extendedLevelsList);
                    returnList.Add(returnLevelGroup);
                    currentGroup.extendedLevelsList.Clear();
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

        internal static void CreateTerminalEventNode(TerminalKeyword verbKeyword, string eventString)
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