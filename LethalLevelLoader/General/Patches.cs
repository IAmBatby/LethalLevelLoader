using Discord;
using DunGen;
using DunGen.Graph;
using HarmonyLib;
using LethalLevelLoader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TMPro;
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
                AssetBundleLoader.CreateLoadingBundlesHeaderText(__instance);
                if (__instance.TryGetComponent(out AudioSource audioSource))
                    OriginalContent.AudioMixers.Add(audioSource.outputAudioMixerGroup.audioMixer);

                AssetBundleLoader.LoadBundles(__instance);
                AssetBundleLoader.onBundlesFinishedLoading += AssetBundleLoader.LoadContentInBundles;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PreInitSceneScript), "ChooseLaunchOption")]
        [HarmonyPrefix]
        internal static bool PreInitSceneScriptChooseLaunchOption_Prefix()
        {
            return ((AssetBundleLoader.loadedFilesTotal - AssetBundleLoader.loadingAssetBundles.Count) == AssetBundleLoader.loadedFilesTotal);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_Prefix(GameNetworkManager __instance)
        {
            if (Plugin.hasVanillaBeenPatched == false)
            {
                foreach (NetworkPrefab networkPrefab in __instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.m_Prefabs)
                    if (networkPrefab.Prefab.name.Contains("EntranceTeleport"))
                        if (networkPrefab.Prefab.GetComponent<AudioSource>() != null)
                            OriginalContent.AudioMixers.Add(networkPrefab.Prefab.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                GameObject networkManagerPrefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab("LethalLevelLoaderNetworkManagerTest");
                networkManagerPrefab.AddComponent<LethalLevelLoaderNetworkManager>();
                networkManagerPrefab.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
                networkManagerPrefab.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
                networkManagerPrefab.GetComponent<NetworkObject>().DestroyWithScene = false;
                GameObject.DontDestroyOnLoad(networkManagerPrefab);
                LethalLevelLoaderNetworkManager.networkingManagerPrefab = networkManagerPrefab;
              
                AssetBundleLoader.RegisterCustomContent(__instance.GetComponent<NetworkManager>());
                LethalLevelLoaderNetworkManager.RegisterPrefabs(__instance.GetComponent<NetworkManager>());
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPrefix]
        internal static void StartOfRoundAwake_Prefix(StartOfRound __instance)
        {
            if (Plugin.hasVanillaBeenPatched == false)
                ContentExtractor.TryScrapeVanillaItems(__instance);
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

                RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.ToList().AddItem(RoundManager.Instance.firstTimeDungeonAudios[0]).ToArray();
                ContentExtractor.TryScrapeVanillaContent(__instance);
                TerminalManager.CacheTerminalReferences();

                AssetBundleLoader.CreateVanillaExtendedDungeonFlows();
                AssetBundleLoader.CreateVanillaExtendedLevels(StartOfRound.Instance);
                AssetBundleLoader.InitializeBundles();

                string debugString = "LethalLevelLoader Loaded The Following ExtendedLevels:"+ "\n";
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    debugString += (PatchedContent.ExtendedLevels.IndexOf(extendedLevel) + 1) + ". " + extendedLevel.selectableLevel.PlanetName + " (" + extendedLevel.levelType + ")" + "\n";
                DebugHelper.Log(debugString);

                debugString = "LethalLevelLoader Loaded The Following ExtendedDungeonFlows:" + "\n";
                foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.ExtendedDungeonFlows)
                    debugString += (PatchedContent.ExtendedDungeonFlows.IndexOf(extendedDungeonFlow) + 1) + ". " + extendedDungeonFlow.dungeonDisplayName + " (" + extendedDungeonFlow.dungeonFlow.name + ") (" + extendedDungeonFlow.dungeonType + ")" + "\n";
                DebugHelper.Log(debugString);
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

                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneLoaded += EventPatches.OnSceneLoaded;

                foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                    audioSource.spatialize = false;

                Plugin.hasVanillaBeenPatched = true;
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
            if (node == TerminalManager.moonsKeyword.specialKeywordResult)
            {
                TerminalManager.RefreshExtendedLevelGroups();
                node.displayText = TerminalManager.GetMoonsTerminalText();
            }
            else if (__instance.currentNode == TerminalManager.moonsKeyword.specialKeywordResult)
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.routeNode == node && extendedLevel.isLocked == true)
                        TerminalManager.SwapRouteNodeToLockedNode(extendedLevel, ref node);
        }

        //Called via SceneManager event.
        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (LevelManager.CurrentExtendedLevel != null && LevelManager.CurrentExtendedLevel.IsLoaded)
                if (LevelManager.CurrentExtendedLevel.levelType == ContentType.Custom)
                    foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.selectableLevel.sceneName).GetRootGameObjects())
                    {
                        LevelLoader.UpdateStoryLogs(LevelManager.CurrentExtendedLevel, rootObject);
                        ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
                    }
            
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static void DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (LevelManager.TryGetExtendedLevel(RoundManager.Instance.currentLevel, out ExtendedLevel currentExtendedLevel))
                DungeonLoader.PrepareDungeon();
            LevelManager.LogDayHistory();
        }

        //Basegame has a bug where it stops listening before it gets the Complete call, so this is just a fixed version of the basegame function.
        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Generator_OnGenerationStatusChanged")]
        [HarmonyPrefix]
        internal static bool OnGenerationStatusChanged_Prefix(RoundManager __instance, GenerationStatus status)
        {
            if (status == GenerationStatus.Complete && !__instance.dungeonCompletedGenerating)
            {
                __instance.FinishGeneratingLevel();
                __instance.dungeonGenerator.Generator.OnGenerationStatusChanged -= __instance.Generator_OnGenerationStatusChanged;
                Debug.Log("Dungeon has finished generating on this client after multiple frames");
            }
            return (false);
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenerateNewLevelClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .SearchForward(instructions => instructions.Calls(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(InjectHostDungeonFlowSelection))))
                .Advance(-1)
                .SetInstruction(new CodeInstruction(OpCodes.Nop));
            return (codeMatcher.InstructionEnumeration());
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenerateNewFloorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .SearchForward(instructions => instructions.Calls(AccessTools.Method(typeof(RuntimeDungeon), nameof(RuntimeDungeon.Generate))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(InjectHostDungeonSizeSelection))))
                .Advance(-1)
                .SetInstruction(new CodeInstruction(OpCodes.Nop));
            return (codeMatcher.InstructionEnumeration());
        }

        //Called via Transpiler.
        public static void InjectHostDungeonSizeSelection(RoundManager roundManager)
        {
            if (LevelManager.CurrentExtendedLevel != null)
                LethalLevelLoaderNetworkManager.Instance.GetDungeonFlowSizeServerRpc();
            else
                roundManager.dungeonGenerator.Generate();
        }

        //Called via Transpiler.
        internal static void InjectHostDungeonFlowSelection()
        {
            if (LevelManager.CurrentExtendedLevel != null)
            {
                DungeonManager.TryAddCurrentVanillaLevelDungeonFlow(RoundManager.Instance.dungeonGenerator.Generator, LevelManager.CurrentExtendedLevel);
                DungeonLoader.SelectDungeon();
            }
            else
                RoundManager.Instance.GenerateNewFloor();
        }
    }
}
