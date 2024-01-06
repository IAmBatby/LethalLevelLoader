using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalLevelLoader
{
    public class Terminal_Patch
    {
        private static Terminal _terminal;
        public static Terminal Terminal
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

        //Hardcoded References To Important Base-Game TerminalKeywords;
        public static TerminalKeyword RouteKeyword => GetTerminalKeywordFromIndex(26);
        public static TerminalKeyword InfoKeyword => GetTerminalKeywordFromIndex(6);
        public static TerminalKeyword ConfirmKeyword => GetTerminalKeywordFromIndex(3);
        public static TerminalKeyword DenyKeyword => GetTerminalKeywordFromIndex(4);
        //This isn't anywhere easy to grab so we grab it from Vow's Route.
        public static TerminalNode CancelRouteNode
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
        public static void TextPostProcess_PreFix(ref string modifiedDisplayText)
        {
            if (modifiedDisplayText.Contains("Welcome to the exomoons catalogue"))
            {
                modifiedDisplayText = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company building   //   Buying at [companyBuyingPercent].\r\n\r\n";

                List<ExtendedLevel> tweakedVanillaLevelsList = new List<ExtendedLevel>(SelectableLevel_Patch.vanillaLevelsList);

                tweakedVanillaLevelsList.RemoveAt(3);
                tweakedVanillaLevelsList.Insert(3, tweakedVanillaLevelsList[6]);
                tweakedVanillaLevelsList.RemoveAt(7);
                tweakedVanillaLevelsList.Insert(5, null);

                modifiedDisplayText += GetMoonCatalogDisplayListings(tweakedVanillaLevelsList);
                modifiedDisplayText += GetMoonCatalogDisplayListings(SelectableLevel_Patch.customLevelsList);
                modifiedDisplayText += "\r\n";
            }
        }

        //This is some abslolute super arbitary wizardry to replicate basegame >moons command
        public static string GetMoonCatalogDisplayListings(List<ExtendedLevel> extendedLevels)
        {
            string returnString = string.Empty;
            string previousLevelSource = string.Empty;

            int seperationCountMax = 3;
            int seperationCount = 0;

            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if (extendedLevel != null)
                {
                    if (previousLevelSource != string.Empty && previousLevelSource != extendedLevel.sourceName)
                    {
                        returnString += "\n";
                        seperationCount = 0;
                    }

                    returnString += "* " + extendedLevel.NumberlessPlanetName + " " + GetMoonConditions(extendedLevel.selectableLevel) + "\n";
                    previousLevelSource = extendedLevel.sourceName;
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

        //Just returns the level weather with a space and ().
        public static string GetMoonConditions(SelectableLevel selectableLevel)
        {
            string returnString = string.Empty;

            if (selectableLevel != null)
                if (selectableLevel.currentWeather != LevelWeatherType.None)
                    returnString = "(" + selectableLevel.currentWeather.ToString() + ")";

            return (returnString);
        }

        public static void CreateLevelTerminalData(ExtendedLevel extendedLevel)
        {
            TerminalKeyword tempRouteKeyword = GetTerminalKeywordFromIndex(26);
            TerminalKeyword tempInfoKeyword = GetTerminalKeywordFromIndex(6);
            DebugHelper.Log("Temp Route Keyword Is: " + (tempRouteKeyword != null).ToString());
            DebugHelper.Log("Temp Route Keyword Is: " + (tempInfoKeyword != null).ToString());


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

        public static TerminalKeyword GetTerminalKeywordFromIndex(int index)
        {
            if (Terminal != null)
                return (Terminal.terminalNodes.allKeywords[index]);
            else
                return (null);
        }
    }
}