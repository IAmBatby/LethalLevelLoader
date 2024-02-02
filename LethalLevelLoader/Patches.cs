using Discord;
using DunGen;
using DunGen.Graph;
using HarmonyLib;
using LethalLevelLoader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Device;
using UnityEngine.SceneManagement;
using NetworkManager = Unity.Netcode.NetworkManager;

namespace LethalLevelLoader
{
    internal static class Patches
    {
        internal const int harmonyPriority = 200;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static void PreInitSceneScriptAwake_Prefix(PreInitSceneScript __instance)
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                if (__instance.TryGetComponent(out AudioSource audioSource))
                    OriginalContent.AudioMixers.Add(audioSource.outputAudioMixerGroup.audioMixer);
                AssetBundleLoader.LoadBundles();
                AssetBundleLoader.LoadContentInBundles();
            }
        }

        [HarmonyPriority(1)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_FirstPrefix(GameNetworkManager __instance)
        {
            foreach (NetworkPrefab networkPrefab in __instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.m_Prefabs)
            {
                if (networkPrefab.Prefab.name.Contains("EntranceTeleport"))
                    if (networkPrefab.Prefab.GetComponent<AudioSource>() != null)
                    {
                        OriginalContent.AudioMixers.Add(networkPrefab.Prefab.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);
                        return;
                    }
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_Prefix(GameNetworkManager __instance)
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                AssetBundleLoader.RegisterCustomContent(__instance.GetComponent<NetworkManager>());
                NetworkManager_Patch.RegisterPrefabs(__instance.GetComponent<NetworkManager>());
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        internal static void RoundManagerAwake_Postfix(RoundManager __instance)
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.ToList().AddItem(RoundManager.Instance.firstTimeDungeonAudios[0]).ToArray();
                ContentExtractor.TryScrapeVanillaContent(__instance);
                Terminal_Patch.CacheTerminalReferences();

                AssetBundleLoader.CreateVanillaExtendedDungeonFlows();
                AssetBundleLoader.CreateVanillaExtendedLevels(StartOfRound.Instance);
                AssetBundleLoader.InitializeBundles();

                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    DebugHelper.Log(extendedLevel.levelType + " - " + extendedLevel.NumberlessPlanetName);
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        internal static void RoundManagerStart_Prefix()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {

                foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                    ContentRestorer.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                    ContentRestorer.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                Terminal_Patch.CreateMoonsFilterTerminalAssets();
                Terminal_Patch.CreateExtendedLevelGroups();

                ConfigLoader.BindConfigs();
                LethalLevelLoaderPlugin.hasVanillaBeenPatched = true;
            }

            SelectableLevel_Patch.PatchVanillaLevelLists();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "ParseWord")]
        [HarmonyPostfix]
        internal static void TerminalParseWord_Postfix(Terminal __instance, ref TerminalKeyword __result, string playerWord)
        {
            if (__result != null)
            {
                TerminalKeyword newKeyword = Terminal_Patch.TryFindAlternativeNoun(__instance, __result, playerWord);
                if (newKeyword != null)
                    __result = newKeyword;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPrefix]
        internal static bool TerminalRunTerminalEvents_Prefix(TerminalNode node)
        {
            return (Terminal_Patch.RunLethalLevelLoaderTerminalEvents(node));
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPrefix]
        internal static void TerminalLoadNewNode_Prefix(Terminal __instance, ref TerminalNode node)
        {
            if (node != null && Terminal_Patch.Terminal.currentNode != null)
                DebugHelper.Log(node.name + " | " + Terminal_Patch.Terminal.currentNode.name);
            if (node == Terminal_Patch.moonsKeyword.specialKeywordResult)
            {
                DebugHelper.Log("LoadNewNode Prefix! Node Is: " + node.name);
                Terminal_Patch.RefreshExtendedLevelGroups();
                node.displayText = Terminal_Patch.GetMoonsTerminalText();
            }
            else if (__instance.currentNode == Terminal_Patch.moonsKeyword.specialKeywordResult)
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.routeNode == node && extendedLevel.isLocked == true)
                        Terminal_Patch.SwapRouteNodeToLockedNode(extendedLevel, ref node);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        internal static void TerminalTextPostProcess_Prefix(ref string modifiedDisplayText)
        {

        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPrefix]
        internal static void OnLoadComplete1_Prefix(string sceneName)
        {
            ExtendedLevel currentExtendedLevel = SelectableLevel_Patch.GetExtendedLevel(StartOfRound.Instance.currentLevel);
            if (currentExtendedLevel != null && currentExtendedLevel.selectableLevel.sceneName == sceneName && SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                foreach (GameObject rootObject in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
                {
                    if (currentExtendedLevel.levelType == ContentType.Custom)
                    {
                        LevelLoader.UpdateStoryLogs(currentExtendedLevel, rootObject);
                        ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
                    }
                    else if (currentExtendedLevel.levelType == ContentType.Vanilla)
                    {
                        //foreach (ReverbPreset reverbPreset in Resources.FindObjectsOfTypeAll<ReverbPreset>())
                            //ContentExtractor.TryAddReference(OriginalContent.ReverbPresets, reverbPreset);
                    }
                }
            }
        }


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPrefix]
        internal static void RoundManagerGenerateNewFloor_Prefix(RoundManager __instance)
        {

        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static bool DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (SelectableLevel_Patch.TryGetExtendedLevel(RoundManager.Instance.currentLevel, out ExtendedLevel currentExtendedLevel))
                DungeonLoader.PrepareDungeon(__instance, currentExtendedLevel);

            SelectableLevel_Patch.LogDayHistory();

            return (true);
        }
    }
}
