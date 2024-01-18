using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

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
            SetMoonsTerminalText(ref modifiedDisplayText);
        }

        internal static void SetMoonsTerminalText(ref string modifiedDisplayText)
        {
            if (modifiedDisplayText.Contains("Welcome to the exomoons catalogue"))
            {
                modifiedDisplayText = "\n" + "\n" + "\n" + "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company building   //   Buying at [companyBuyingPercent].\r\n\r\n";
                modifiedDisplayText += GetMoonCatalogDisplayListings(Terminal_Patch.Terminal.moonsCatalogueList.ToList()) + "\r\n";
            }
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings(List<SelectableLevel> selectableLevels)
        {
            string returnString = string.Empty;
            string previousLevelSource = string.Empty;

            int seperationCountMax = 3;
            int seperationCount = 0;

            foreach (SelectableLevel selectableLevel in selectableLevels)
            {
                if (SelectableLevel_Patch.TryGetExtendedLevel(selectableLevel, out ExtendedLevel extendedLevel))
                {
                    if (Terminal.moonsCatalogueList[5] == selectableLevel) //Hardcoded hotfix to gap Assurance and Vow
                    {
                        returnString += "\n";
                        seperationCount = 0;
                    }
                    if (previousLevelSource != string.Empty && previousLevelSource != extendedLevel.contentSourceName)
                    {
                        returnString += "\n";
                        seperationCount = 0;
                    }

                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetExtendedLevelPreviewInfo(extendedLevel) + "\n";
                    previousLevelSource = extendedLevel.contentSourceName;
                }

                seperationCount++;
                if (seperationCount == seperationCountMax)
                {
                    returnString += "\n";
                    seperationCount = 0;
                }
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
            string returnString = string.Empty;

            if (selectableLevel != null)
                if (selectableLevel.currentWeather != LevelWeatherType.None)
                    returnString = "(" + selectableLevel.currentWeather.ToString() + ")";

            return (returnString);
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
            string[] filterOptions = new string[] { "Weather", "Price", "Difficulty", "None" };
            TerminalKeyword toggleKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();

            toggleKeyword.word = "toggle";
            toggleKeyword.isVerb = true;

            foreach (string filterOption in filterOptions)
            {
                TerminalKeyword filterKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                TerminalNode filterNode = ScriptableObject.CreateInstance<TerminalNode>();

                filterKeyword.word = filterOption.ToLower();
                filterKeyword.defaultVerb = toggleKeyword;

                filterNode.displayText = string.Empty;
                filterNode.terminalEvent = filterOption;
                filterNode.clearPreviousText = false;
                filterNode.maxCharactersToType = 25;
                filterNode.buyItemIndex = -1;
                filterNode.buyRerouteToMoon = -1;
                filterNode.displayPlanetInfo = -1;
                filterNode.lockedInDemo = false;
                filterNode.shipUnlockableID = -1;
                filterNode.itemCost = 0;
                filterNode.creatureFileID = -1;
                filterNode.storyLogFileID = -1;
                filterNode.playSyncedClip = -1;

                CompatibleNoun newCompatibleNoun = new CompatibleNoun();
                newCompatibleNoun.noun = filterKeyword;
                newCompatibleNoun.result = filterNode;

                toggleKeyword.compatibleNouns = toggleKeyword.compatibleNouns.AddItem(newCompatibleNoun).ToArray();
                Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(filterKeyword).ToArray();
            }

            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddItem(toggleKeyword).ToArray();

        }


        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        internal static void RunTerminalEvents(TerminalNode node)
        {
            bool loadMoonsNode = false;
            if (node.terminalEvent == "Weather")
            {
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Weather;
                loadMoonsNode = true;
            }
            else if (node.terminalEvent == "Price")
            {
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Price;
                loadMoonsNode = true;
            }
            else if (node.terminalEvent == "Difficulty")
            {
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Difficulty;
                loadMoonsNode = true;
            }
            else if (node.terminalEvent == "None")
            {
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Empty;
                loadMoonsNode = true;
            }

            if (loadMoonsNode == true)
            {
                float cachedScrollbarValue = Terminal.scrollBarVertical.value;
                Terminal.LoadNewNode(MoonsKeyword.specialKeywordResult);
                Terminal.scrollBarVertical.value = cachedScrollbarValue;
            }
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