using Discord;
using DunGen;
using DunGen.Graph;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader.Tools;
using LethalLib.Modules;
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

        internal static string delayedSceneLoadingName = string.Empty;

        internal static List<string> allSceneNamesCalledToLoad = new List<string>();

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
            //return ((AssetBundleLoader.loadedFilesTotal - AssetBundleLoader.loadingAssetBundles.Count) == AssetBundleLoader.loadedFilesTotal);
            return true;
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(SceneManager), "LoadScene", new Type[] {typeof(string)})]
        [HarmonyPrefix]
        internal static bool SceneManagerLoadScene(string sceneName)
        {
            if (allSceneNamesCalledToLoad.Count == 0)
                allSceneNamesCalledToLoad.Add(SceneManager.GetActiveScene().name);
            if (SceneManager.GetSceneByName(sceneName) != null)
                allSceneNamesCalledToLoad.Add(sceneName);

            if (sceneName == "MainMenu" && !allSceneNamesCalledToLoad.Contains("InitSceneLaunchOptions"))
            {
                DebugHelper.LogError("SceneManager has been told to load Main Menu without ever loading InitSceneLaunchOptions. This will break LethalLevelLoader. This is likely due to a \"Skip to Main Menu\" mod.");
                return (false);
            }

            if (AssetBundleLoader.loadingStatus == AssetBundleLoader.LoadingStatus.Loading)
            {
                DebugHelper.LogWarning("SceneManager has attempted to load " + sceneName + " Scene before AssetBundles have finished loading. Pausing request until LethalLeveLoader is ready to proceed.");
                delayedSceneLoadingName = sceneName;
                AssetBundleLoader.onBundlesFinishedLoading -= LoadMainMenu;
                AssetBundleLoader.onBundlesFinishedLoading += LoadMainMenu;

                return (false);
            }
            return (true);
        }

        internal static void LoadMainMenu()
        {
            DebugHelper.LogWarning("Proceeding with the loading of " + delayedSceneLoadingName + " Scene as LethalLevelLoader has finished loading AssetBundles.");
            if (delayedSceneLoadingName != string.Empty)
                SceneManager.LoadScene(delayedSceneLoadingName);
            delayedSceneLoadingName = string.Empty;
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
            if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsServer)
            {
                LethalLevelLoaderNetworkManager lethalLevelLoaderNetworkManager = GameObject.Instantiate(LethalLevelLoaderNetworkManager.networkingManagerPrefab).GetComponent<LethalLevelLoaderNetworkManager>();
                lethalLevelLoaderNetworkManager.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
            }

            RoundManager.Instance.firstTimeDungeonAudios = RoundManager.Instance.firstTimeDungeonAudios.ToList().AddItem(RoundManager.Instance.firstTimeDungeonAudios[0]).ToArray();

            if (Plugin.hasVanillaBeenPatched == false)
            {
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
                ContentRestorer.DestroyRestoredAssets();

                foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                    ContentRestorer.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneLoaded += EventPatches.OnSceneLoaded;
            }

            foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                audioSource.spatialize = false;

            ConfigLoader.BindConfigs();

            LevelManager.ValidateLevelLists();

            LevelManager.PatchVanillaLevelLists();
            DungeonManager.PatchVanillaDungeonLists();

            LevelManager.RefreshCustomExtendedLevelIDs();

            LevelManager.PopulateDynamicRiskLevelDictionary();
            LevelManager.AssignCalculatedRiskLevels();

            TerminalManager.CreateExtendedLevelGroups();


            if (Plugin.hasVanillaBeenPatched == false)
            {
                TerminalManager.CreateMoonsFilterTerminalAssets();
            }

            if (LevelManager.invalidSaveLevelID != -1 && StartOfRound.Instance.levels.Length > LevelManager.invalidSaveLevelID)
            {
                DebugHelper.Log("Setting CurrentLevel to previously saved ID that was not loaded at the time of save loading.");
                DebugHelper.Log(LevelManager.invalidSaveLevelID + " / " + (StartOfRound.Instance.levels.Length));
                StartOfRound.Instance.ChangeLevelServerRpc(LevelManager.invalidSaveLevelID, TerminalManager.Terminal.groupCredits);
                LevelManager.invalidSaveLevelID = -1;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        internal static void RoundManagerStart_Postfix()
        {
            if (Plugin.hasVanillaBeenPatched == false)
            {
                ContentExtractor.TryScrapeCustomContent();

                Plugin.hasVanillaBeenPatched = true;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(TimeOfDay), "Start")]
        [HarmonyPostfix]
        internal static void TimeOfDayStart_Postfix(TimeOfDay __instance)
        {
            AssetBundleLoader.CreateVanillaExtendedWeatherEffects(StartOfRound.Instance, __instance);
            WeatherManager.PopulateVanillaExtendedWeatherEffectsDictionary();
            WeatherManager.PopulateExtendedLevelEnabledExtendedWeatherEffects();
            StartOfRound.Instance.SetPlanetsWeather(0);
        }


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
        [HarmonyPrefix]
        internal static void StartOfRoundChangeLevel_Prefix(ref int levelID)
        {
            if (levelID >= StartOfRound.Instance.levels.Length)
            {
                DebugHelper.LogWarning("Lethal Company attempted to load a saved current level that has not yet been loaded");
                DebugHelper.LogWarning(levelID + " / " + (StartOfRound.Instance.levels.Length));
                LevelManager.invalidSaveLevelID = levelID;
                levelID = 0;
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
        [HarmonyPrefix]
        internal static bool StartOfRoundSetPlanetsWeather_Prefix(int connectedPlayersOnServer)
        {
            if (WeatherManager.vanillaExtendedWeatherEffectsDictionary.Count != 0)
            {
                WeatherManager.SetExtendedLevelsExtendedWeatherEffect(connectedPlayersOnServer);
                return (false);
            }
            else
                return (true);
        }


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        internal static void TerminalStart_Postfix()
        {
            LevelManager.RefreshLethalExpansionMoons();
            StartOfRound.Instance.SetPlanetsWeather();

            List<ExtendedLevel> levels = PatchedContent.ExtendedLevels.OrderBy(o => o.CalculatedDifficultyRating).ToList();
            foreach (ExtendedLevel extendedLevel in levels)
                LevelManager.CalculateExtendedLevelDifficultyRating(extendedLevel, true);
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

        internal static bool ranLethalLevelLoaderTerminalEvent;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPrefix]
        internal static bool TerminalRunTerminalEvents_Prefix(Terminal __instance, TerminalNode node)
        {
            if (__instance.currentNode != TerminalManager.moonsKeyword.specialKeywordResult)
            {
                ranLethalLevelLoaderTerminalEvent = false;
                return (true);
            }
            else
            {
                ranLethalLevelLoaderTerminalEvent = !TerminalManager.RunLethalLevelLoaderTerminalEvents(node);
                return (!ranLethalLevelLoaderTerminalEvent);
            }
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
                    if (extendedLevel.RouteNode == node && extendedLevel.isLocked == true)
                        TerminalManager.SwapRouteNodeToLockedNode(extendedLevel, ref node);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPostfix]
        internal static void TerminalLoadNewNode_Postfix(Terminal __instance, ref TerminalNode node)
        {
            if (ranLethalLevelLoaderTerminalEvent == true)
                __instance.currentNode = TerminalManager.moonsKeyword.specialKeywordResult;
        }

        //Called via SceneManager event.
        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (LevelManager.CurrentExtendedLevel != null && LevelManager.CurrentExtendedLevel.IsLoadedLevel)
                if (LevelManager.CurrentExtendedLevel.levelType == ContentType.Custom && LevelManager.CurrentExtendedLevel.isLethalExpansion == false)
                {
                    DebugHelper.DebugSpawnScrap(LevelManager.CurrentExtendedLevel);
                    foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.selectableLevel.sceneName).GetRootGameObjects())
                    {
                        LevelLoader.UpdateStoryLogs(LevelManager.CurrentExtendedLevel, rootObject);
                        ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
                    }
                }
            
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static void DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (LevelManager.CurrentExtendedLevel != null)
                DungeonLoader.PrepareDungeon();
            LevelManager.LogDayHistory();

            if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow == null)
                DebugHelper.LogError("Critical Failure! DungeonGenerator DungeonFlow Is Null!");
        }

        //Basegame has a bug where it stops listening before it gets the Complete call, so this is just a fixed version of the basegame function.
        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "Generator_OnGenerationStatusChanged")]
        [HarmonyPrefix]
        internal static bool OnGenerationStatusChanged_Prefix(RoundManager __instance, GenerationStatus status)
        {
            DebugHelper.Log(status.ToString());
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

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(LethalLib.Modules.Dungeon), "RoundManager_Start")]
        [HarmonyPrefix]
        internal static bool Dungeon_Start_Prefix(On.RoundManager.orig_Start orig, RoundManager self)
        {
            DebugHelper.LogWarning("Disabling LethalLib Dungeon.RoundManager_Start() Function To Prevent Conflicts");
            orig(self);
            return (false);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(LethalLib.Modules.Dungeon), "RoundManager_GenerateNewFloor")]
        [HarmonyPrefix]
        internal static bool Dungeon_GenerateNewFloor_Prefix(On.RoundManager.orig_GenerateNewFloor orig, RoundManager self)
        {
            DebugHelper.LogWarning("Disabling LethalLib Dungeon.RoundManager_GenerateNewFloor() Function To Prevent Conflicts");
            orig(self);
            return (false);
        }

        internal static GameObject previousHit;
        internal static FootstepSurface previouslyAssignedFootstepSurface;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PlayerControllerB), "GetCurrentMaterialStandingOn")]
        [HarmonyPrefix]
        internal static bool PlayerControllerBGetCurrentMaterialStandingOn_Prefix(PlayerControllerB __instance)
        {
            if (LevelManager.CurrentExtendedLevel.extendedFootstepSurfaces.Count != 0)
                if (Physics.Raycast(new Ray(__instance.thisPlayerBody.position + Vector3.up, -Vector3.up), out RaycastHit hit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
                    if (hit.collider.gameObject == previousHit)
                        return (false);
            return (true);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PlayerControllerB), "GetCurrentMaterialStandingOn")]
        [HarmonyPostfix]
        internal static void PlayerControllerBGetCurrentMaterialStandingOn_Postfix(PlayerControllerB __instance)
        {
            if (LevelManager.CurrentExtendedLevel.extendedFootstepSurfaces.Count != 0)
                if (Physics.Raycast(new Ray(__instance.thisPlayerBody.position + Vector3.up, -Vector3.up), out RaycastHit hit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
                    if (hit.collider.gameObject != previousHit || previousHit == null)
                    {
                        previousHit = hit.collider.gameObject;
                        if (hit.collider.CompareTag("Untagged") || !LevelManager.cachedFootstepSurfaceTagsList.Contains(hit.collider.tag))
                            if (hit.collider.gameObject.TryGetComponent(out MeshRenderer meshRenderer))
                                foreach (Material material in meshRenderer.sharedMaterials)
                                    foreach (ExtendedFootstepSurface extendedFootstepSurface in LevelManager.CurrentExtendedLevel.extendedFootstepSurfaces)
                                        foreach (Material associatedMaterial in extendedFootstepSurface.associatedMaterials)
                                            if (material.name == associatedMaterial.name)
                                            {
                                                __instance.currentFootstepSurfaceIndex = extendedFootstepSurface.arrayIndex;
                                                return;
                                            }
                    }
        }
    }
}
