using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LethalLevelLoader
{
    public class DungeonManager : ExtendedContentManager<ExtendedDungeonFlow, DungeonFlow>
    {
        public static ExtendedDungeonFlow CurrentExtendedDungeonFlow
        {
            get
            {
                ExtendedDungeonFlow returnFlow = null;
                if (Patches.RoundManager != null && Patches.RoundManager.dungeonGenerator != null)
                    if (TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow flow))
                        returnFlow = flow;
                return (returnFlow);
            }
        }
        public static DungeonEvents GlobalDungeonEvents = new DungeonEvents();

        protected override List<DungeonFlow> GetVanillaContent() => new List<DungeonFlow>(RoundManager.dungeonFlowTypes.Select(d => d.dungeonFlow));

        protected override ExtendedDungeonFlow ExtendVanillaContent(DungeonFlow content)
        {
            (string, AudioClip) refs = default;
            if (content.name.Contains("Level1"))
                refs = ("Facility", RoundManager.firstTimeDungeonAudios[0]);
            else if (content.name.Contains("Level2"))
                refs = ("Haunted Mansion", RoundManager.firstTimeDungeonAudios[1]);
            else if (content.name.Contains("Level3"))
                refs = ("Mineshaft", RoundManager.firstTimeDungeonAudios[2]);

            ExtendedDungeonFlow extendedDungeonFlow = ExtendedDungeonFlow.Create(content, refs.Item2);
            extendedDungeonFlow.DungeonName = refs.Item1;
            return (extendedDungeonFlow);
        }

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);
            List<IndoorMapType> indoorMapTypes = new List<IndoorMapType>();

            List<ExtendedDungeonFlow> flows = new List<ExtendedDungeonFlow>(PatchedContent.ExtendedDungeonFlows);
            for (int i = 0; i < RoundManager.dungeonFlowTypes.Length; i++)
            {
                flows[i].SetGameID(indoorMapTypes.Count);
                indoorMapTypes.Add(Utilities.Create(flows[i].DungeonFlow, flows[i].MapTileSize, flows[i].FirstTimeDungeonAudio));
            }

            RoundManager.dungeonFlowTypes = indoorMapTypes.ToArray();
            RoundManager.firstTimeDungeonAudios = flows.Select(f => f.FirstTimeDungeonAudio).ToArray();
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        public static List<ExtendedDungeonFlowWithRarity> GetValidExtendedDungeonFlows(ExtendedLevel extendedLevel, bool debugResults)
        {
            DebugStopwatch.StartStopWatch("Get Valid ExtendedDungeonFlows");
            List<ExtendedDungeonFlowWithRarity> returnExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();
            List<ExtendedDungeonFlowWithRarity> potentialExtendedDungeonFlowsList = new List<ExtendedDungeonFlowWithRarity>();

            foreach (ExtendedDungeonFlow vanillaDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(vanillaDungeonFlow, 0));

            //Add Custom DungeonFlows
            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                potentialExtendedDungeonFlowsList.Add(new ExtendedDungeonFlowWithRarity(customDungeonFlow, 0));

            foreach (ExtendedDungeonFlowWithRarity customDungeonFlow in new List<ExtendedDungeonFlowWithRarity>(potentialExtendedDungeonFlowsList))
            {
                customDungeonFlow.rarity = customDungeonFlow.extendedDungeonFlow.LevelMatchingProperties.GetDynamicRarity(extendedLevel);
                if (customDungeonFlow.rarity != 0)
                    returnExtendedDungeonFlowsList.Add(customDungeonFlow);
            }

            if (debugResults == true)
            {
                string debugString = "ExtendedLevel <-> ExtendedDungeonFlow Dynamic Matching Report." + "\n\n";

                debugString += "Info For ExtendedLevel: " + extendedLevel.name + " | Planet Name: " + extendedLevel.NumberlessPlanetName + " | Content Tags: ";
                foreach (ContentTag tag in extendedLevel.ContentTags)
                    debugString += tag.contentTagName + ", ";
                debugString = debugString.TrimEnd([',', ' ']);
                debugString += " | Route Price: " + extendedLevel.PurchasePrice + " | Current Weather: " + extendedLevel.SelectableLevel.currentWeather.ToString();
                debugString += "\n";

                List<ExtendedDungeonFlow> viableDungeonFlows = returnExtendedDungeonFlowsList.Select(d => d.extendedDungeonFlow).ToList();
                debugString += "Unviable ExtendedDungeonFlows: ";
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in potentialExtendedDungeonFlowsList)
                    if (!viableDungeonFlows.Contains(extendedDungeonFlowWithRarity.extendedDungeonFlow))
                        debugString += extendedDungeonFlowWithRarity.extendedDungeonFlow.DungeonName + ", ";
                debugString = debugString.TrimEnd([',', ' ']);
                debugString += "\n";

                returnExtendedDungeonFlowsList = returnExtendedDungeonFlowsList.OrderBy(e => e.rarity).Reverse().ToList();

                debugString += "Viable ExtendedDungeonFlows: ";
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in returnExtendedDungeonFlowsList)
                    debugString += extendedDungeonFlowWithRarity.extendedDungeonFlow.DungeonName + " (" + extendedDungeonFlowWithRarity.rarity + ")" + ", ";
                debugString = debugString.TrimEnd([',', ' ']);

                DebugHelper.Log(debugString + "\n", DebugType.User);
            }

            DebugStopwatch.StopStopWatch("Get Valid ExtendedDungeonFlows");

            return (returnExtendedDungeonFlowsList);
        }

        internal static bool TryGetExtendedDungeonFlow(DungeonFlow dungeonFlow, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType contentType = ContentType.Any)
        {
            returnExtendedDungeonFlow = null;
            List<ExtendedDungeonFlow> extendedDungeonFlowsList = null;

            if (dungeonFlow == null) return (false);

            if (contentType == ContentType.Any)
                extendedDungeonFlowsList = PatchedContent.ExtendedDungeonFlows;
            else if (contentType == ContentType.Custom)
                extendedDungeonFlowsList = PatchedContent.CustomExtendedDungeonFlows;
            else if (contentType == ContentType.Vanilla)
                extendedDungeonFlowsList = PatchedContent.VanillaExtendedDungeonFlows;

            foreach (ExtendedDungeonFlow extendedDungeonFlow in extendedDungeonFlowsList)
                if (extendedDungeonFlow.DungeonFlow == dungeonFlow)
                    returnExtendedDungeonFlow = extendedDungeonFlow;

            return (returnExtendedDungeonFlow != null);
        }

        internal static bool TryGetExtendedDungeonFlow(IndoorMapType indoorMapType, out ExtendedDungeonFlow returnExtendedDungeonFlow, ContentType contentType = ContentType.Any)
        {
            return (TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out returnExtendedDungeonFlow, contentType));
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedDungeonFlow extendedDungeonFlow)
        {
            return (true, string.Empty);
        }

        protected override void PopulateContentTerminalData(ExtendedDungeonFlow content)
        {

        }
    }
}