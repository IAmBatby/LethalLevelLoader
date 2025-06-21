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
    public class DungeonManager : ExtendedContentManager<ExtendedDungeonFlow, DungeonFlow, DungeonManager>
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

        internal static void PatchVanillaDungeonLists()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                extendedDungeonFlow.DungeonID = Patches.RoundManager.dungeonFlowTypes.Length;
                IndoorMapType newIndoorMapType = new IndoorMapType();
                newIndoorMapType.dungeonFlow = extendedDungeonFlow.DungeonFlow;
                newIndoorMapType.MapTileSize = extendedDungeonFlow.MapTileSize;
                if (extendedDungeonFlow.FirstTimeDungeonAudio != null)
                    newIndoorMapType.firstTimeAudio = extendedDungeonFlow.FirstTimeDungeonAudio;
                Patches.RoundManager.dungeonFlowTypes = Patches.RoundManager.dungeonFlowTypes.AddItem(newIndoorMapType).ToArray();
                if (extendedDungeonFlow.FirstTimeDungeonAudio != null)
                    Patches.RoundManager.firstTimeDungeonAudios = Patches.RoundManager.firstTimeDungeonAudios.AddItem(extendedDungeonFlow.FirstTimeDungeonAudio).ToArray();
            }
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
                debugString += " | Route Price: " + extendedLevel.RoutePrice + " | Current Weather: " + extendedLevel.SelectableLevel.currentWeather.ToString();
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

        internal static void RefreshDungeonFlowIDs()
        {
            //DebugHelper.Log("Re-Adjusting DungeonFlowTypes Array For Late Arriving Vanilla DungeonFlow", DebugType.User);
            
            List<DungeonFlow> cachedDungeonFlowTypes = new List<DungeonFlow>();
            List<IndoorMapType> indoorMapTypes = new List<IndoorMapType>();
            foreach (ExtendedDungeonFlow vanillaDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
            {
                vanillaDungeonFlow.DungeonID = cachedDungeonFlowTypes.Count;
                cachedDungeonFlowTypes.Add(vanillaDungeonFlow.DungeonFlow);
            }
            foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                customDungeonFlow.DungeonID = cachedDungeonFlowTypes.Count;
                cachedDungeonFlowTypes.Add(customDungeonFlow.DungeonFlow);
            }

            foreach (DungeonFlow dungeonFlow in cachedDungeonFlowTypes)
            {
                IndoorMapType newIndoorMapType = new IndoorMapType();
                newIndoorMapType.dungeonFlow = dungeonFlow;
                newIndoorMapType.MapTileSize = 1f;
                indoorMapTypes.Add(newIndoorMapType);
            }
            Patches.RoundManager.dungeonFlowTypes = indoorMapTypes.ToArray();
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
    }
}