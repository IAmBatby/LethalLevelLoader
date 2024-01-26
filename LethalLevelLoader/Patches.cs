using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    internal static class Patches
    {
        internal const int harmonyPriority = 200;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static void PreInitSceneScriptAwake_Prefix()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
                AssetBundleLoader.FindBundles();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_Prefix()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
                AssetBundleLoader.RegisterCustomContent();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        internal static void RoundManagerAwake_Postfix(RoundManager __instance)
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                ContentExtractor.TryScrapeVanillaContent(__instance);

                AssetBundleLoader.CreateVanillaExtendedDungeonFlows();
                AssetBundleLoader.CreateVanillaExtendedLevels(StartOfRound.Instance);
                AssetBundleLoader.InitializeBundles();
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        internal static void RoundManagerStart_Prefix()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                SelectableLevel_Patch.RestoreCustomContent();

                Terminal_Patch.CreateMoonsFilterTerminalAssets();
                Terminal_Patch.CreateExtendedLevelGroups();

                LethalLevelLoaderPlugin.hasVanillaBeenPatched = true;
            }

            SelectableLevel_Patch.PatchVanillaLevelLists();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPrefix]
        internal static bool TerminalRunTerminalEvents_Prefix(TerminalNode node)
        {
            bool result = Terminal_Patch.RunLethalLevelLoaderTerminalEvents(node);

            DebugHelper.Log("TerminalRunTerminalEvents Prefix Returned: " + node);
            return (result);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPrefix]
        internal static void TerminalLoadNewNode_Prefix(TerminalNode node)
        {
            if (node.name == "MoonsCatalogue")
            {
                Terminal_Patch.RefreshExtendedLevelGroups();
                node.displayText = Terminal_Patch.GetMoonsTerminalText();
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        internal static void TerminalTextPostProcess_Prefix(ref string modifiedDisplayText)
        {

        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static bool DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            return (true);
        }
    }
}
