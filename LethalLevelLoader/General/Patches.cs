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
            if (Plugin.hasVanillaBeenPatched == false)
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
            if (Plugin.hasVanillaBeenPatched == false)
            {
                NetworkManager networkManager = __instance.GetComponent<NetworkManager>();

                GameObject networkManagerPrefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab("LethalLevelLoaderNetworkManagerTest");
                networkManagerPrefab.AddComponent<LethalLevelLoaderNetworkManager>();

                /*foreach (NetworkPrefab networkPrefab in new List<NetworkPrefab>(networkManager.NetworkConfig.Prefabs.m_Prefabs))
                    if (networkPrefab.Prefab == networkManagerPrefab)
                        networkManager.NetworkConfig.Prefabs.m_Prefabs.Remove(networkPrefab);*/

                networkManagerPrefab.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
                networkManagerPrefab.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
                networkManagerPrefab.GetComponent<NetworkObject>().DestroyWithScene = false;
                GameObject.DontDestroyOnLoad(networkManagerPrefab);

                //LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(networkManagerPrefab);
                LethalLevelLoaderNetworkManager.networkingManagerPrefab = networkManagerPrefab;
                
                AssetBundleLoader.RegisterCustomContent(__instance.GetComponent<NetworkManager>());
                LethalLevelLoaderNetworkManager.RegisterPrefabs(__instance.GetComponent<NetworkManager>());
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        internal static void RoundManagerAwake_Postfix(RoundManager __instance)
        {
            if (Plugin.hasVanillaBeenPatched == false)
            {
                if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsServer)
                {
                    LethalLevelLoaderNetworkManager lethalLevelLoaderNetworkManager = GameObject.Instantiate(LethalLevelLoaderNetworkManager.networkingManagerPrefab).GetComponent<LethalLevelLoaderNetworkManager>();
                    lethalLevelLoaderNetworkManager.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
                }

                //LethalLevelLoaderNetworkManager.Instance.TestLogServerRpc();

                RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.ToList().AddItem(RoundManager.Instance.firstTimeDungeonAudios[0]).ToArray();
                ContentExtractor.TryScrapeVanillaContent(__instance);
                TerminalManager.CacheTerminalReferences();

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
            if (Plugin.hasVanillaBeenPatched == false)
            {

                foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                    ContentRestorer.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                    ContentRestorer.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                TerminalManager.CreateMoonsFilterTerminalAssets();
                TerminalManager.CreateExtendedLevelGroups();

                ConfigLoader.BindConfigs();
                Plugin.hasVanillaBeenPatched = true;

                foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                    audioSource.spatialize = false;
            }

            LevelManager.PatchVanillaLevelLists();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "ParseWord")]
        [HarmonyPostfix]
        internal static void TerminalParseWord_Postfix(Terminal __instance, ref TerminalKeyword __result, string playerWord)
        {
            if (__result != null)
            {
                TerminalKeyword newKeyword = TerminalManager.TryFindAlternativeNoun(__instance, __result, playerWord);
                if (newKeyword != null)
                    __result = newKeyword;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPrefix]
        internal static bool TerminalRunTerminalEvents_Prefix(TerminalNode node)
        {
            return (TerminalManager.RunLethalLevelLoaderTerminalEvents(node));
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPrefix]
        internal static void TerminalLoadNewNode_Prefix(Terminal __instance, ref TerminalNode node)
        {
            if (node != null && TerminalManager.Terminal.currentNode != null)
                DebugHelper.Log(node.name + " | " + TerminalManager.Terminal.currentNode.name);
            if (node == TerminalManager.moonsKeyword.specialKeywordResult)
            {
                DebugHelper.Log("LoadNewNode Prefix! Node Is: " + node.name);
                TerminalManager.RefreshExtendedLevelGroups();
                node.displayText = TerminalManager.GetMoonsTerminalText();
            }
            else if (__instance.currentNode == TerminalManager.moonsKeyword.specialKeywordResult)
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.routeNode == node && extendedLevel.isLocked == true)
                        TerminalManager.SwapRouteNodeToLockedNode(extendedLevel, ref node);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        internal static void TerminalTextPostProcess_Prefix(ref string modifiedDisplayText)
        {
            LethalLevelLoaderNetworkManager.Instance.TestRpcs();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPrefix]
        internal static void OnLoadComplete1_Prefix(string sceneName)
        {
            ExtendedLevel currentExtendedLevel = LevelManager.GetExtendedLevel(StartOfRound.Instance.currentLevel);
            if (currentExtendedLevel != null && currentExtendedLevel.selectableLevel.sceneName == sceneName && SceneManager.GetSceneByName(sceneName).isLoaded)
                if (currentExtendedLevel.levelType == ContentType.Custom)
                    foreach (GameObject rootObject in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
                    {
                        LevelLoader.UpdateStoryLogs(currentExtendedLevel, rootObject);
                        ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
                    }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static bool DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (LevelManager.TryGetExtendedLevel(RoundManager.Instance.currentLevel, out ExtendedLevel currentExtendedLevel))
                DungeonLoader.SelectDungeon();

            LevelManager.LogDayHistory();

            return (true);
        }
    }
}
