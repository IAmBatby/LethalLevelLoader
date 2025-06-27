using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader.Tools;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using NetworkManager = Unity.Netcode.NetworkManager;

namespace LethalLevelLoader
{
    internal static class Patches
    {
        internal const int priority = 200;

        internal static string delayedSceneLoadingName = string.Empty;

        internal static List<string> allSceneNamesCalledToLoad = new List<string>();

        internal static Dictionary<Camera, float> playerCameras = new Dictionary<Camera, float>();

        internal static bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

        //Caching this because I need it for checks while the local client is disconnecting which may make direct comparisons inconsistent.
        internal static ulong currentClientId;

        //Singletons and such for these are set in each classes Awake function, But they all are accessible on the first awake function of the earliest one of these four managers awake function, so i grab them directly via findobjectoftype to safely access them as early as possible.
        public static StartOfRound StartOfRound { get; internal set; }
        public static RoundManager RoundManager { get; internal set; }
        public static Terminal Terminal { get; internal set; }
        public static TimeOfDay TimeOfDay { get; internal set; }

        public static ExtendedEvent OnBeforeVanillaContentCollected = new ExtendedEvent();
        public static ExtendedEvent OnAfterVanillaContentCollected = new ExtendedEvent();
        public static ExtendedEvent OnAfterCustomContentRestored = new ExtendedEvent();

        [HarmonyPriority(priority)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static void PreInitSceneScriptAwake_Prefix(PreInitSceneScript __instance)
        {
            if (Plugin.IsSetupComplete == false)
            {
                if (__instance.TryGetComponent(out AudioSource audioSource))
                    OriginalContent.AudioMixers.Add(audioSource.outputAudioMixerGroup.audioMixer);
                ContentTagParser.ImportVanillaContentTags();
            }
        }

        [HarmonyPriority(priority)]
        [HarmonyPatch(typeof(SceneManager), "LoadScene", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        internal static bool SceneManagerLoadScene(string sceneName)
        {
            if (allSceneNamesCalledToLoad.Count == 0)
                allSceneNamesCalledToLoad.Add(SceneManager.GetActiveScene().name);
            if (SceneManager.GetSceneByName(sceneName) != null)
                allSceneNamesCalledToLoad.Add(sceneName);

            if (sceneName == "MainMenu" && !allSceneNamesCalledToLoad.Contains("InitSceneLaunchOptions"))
            {
                DebugHelper.LogError("SceneManager has been told to load Main Menu without ever loading InitSceneLaunchOptions. This will break LethalLevelLoader. This is likely due to a \"Skip to Main Menu\" mod.", DebugType.User);
                return (false);
            }

            if (LethalBundleManager.CurrentStatus == LethalBundleManager.ModProcessingStatus.Loading)
            {
                DebugHelper.LogWarning("SceneManager has attempted to load " + sceneName + " Scene before AssetBundles have finished loading. Pausing request until LethalLevelLoader is ready to proceed.", DebugType.User);
                delayedSceneLoadingName = sceneName;
                LethalBundleManager.OnFinishedProcessing.RemoveListener(LoadMainMenu);
                LethalBundleManager.OnFinishedProcessing.AddListener(LoadMainMenu);

                return (false);
            }
            return (true);
        }

        internal static void LoadMainMenu()
        {
            DebugHelper.LogWarning("Proceeding with the loading of " + delayedSceneLoadingName + " Scene as LethalLevelLoader has finished loading AssetBundles.", DebugType.User);
            if (delayedSceneLoadingName != string.Empty)
                SceneManager.LoadScene(delayedSceneLoadingName);
            delayedSceneLoadingName = string.Empty;
        }

        [HarmonyPatch(typeof(NetworkManager), "Awake"), HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void GameNetworkManagerStartFaster_Prefix(NetworkManager __instance)
        {
            ExtendedNetworkManager.TrackVanillaPrefabs();
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Start"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void GameNetworkManagerStart_Prefix(GameNetworkManager __instance)
        {
            if (LethalBundleManager.HasFinalisedFoundContent == false)
                LethalBundleManager.FinialiseFoundContent();
            if (Plugin.IsSetupComplete == false)
            {
                NetworkManager netMan = __instance.GetComponent<NetworkManager>();
                foreach (NetworkPrefab networkPrefab in netMan.NetworkConfig.Prefabs.Prefabs)
                    if (networkPrefab.Prefab.name.Contains("EntranceTeleport") && networkPrefab.Prefab.TryGetComponent(out AudioSource source))
                            OriginalContent.AudioMixers.Add(source.outputAudioMixerGroup.audioMixer);

                ExtendedContentManager.ProcessContentNetworking();
                ExtendedNetworkManager.RegisterPrefabs();
            }
        }

        [HarmonyPatch(typeof(NetworkPrefabHandler), nameof(NetworkPrefabHandler.AddNetworkPrefab)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void OnNetworkPrefabAdded()
        {
            ExtendedNetworkManager.AddNetworkPrefabToRegistry(ExtendedNetworkManager.NetworkManagerInstance.NetworkConfig.Prefabs.Prefabs.Last());
        }

        [HarmonyPatch(typeof(GameNetworkManager), "SaveGameValues"), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void GameNetworkManagerSaveGameValues_Postfix(GameNetworkManager __instance)
        {
            // Vanilla checks
            if (!__instance.isHostingGame || !StartOfRound.Instance.inShipPhase || StartOfRound.Instance.isChallengeFile)
                return;
            SaveManager.SaveGameValues();
        }

        [HarmonyPatch(typeof(StartOfRound), "Awake"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void StartOfRoundAwake_Prefix(StartOfRound __instance)
        {
            Plugin.OnBeforeSetupInvoke();
            //Reference Setup
            StartOfRound = __instance;
            RoundManager = UnityEngine.Object.FindFirstObjectByType<RoundManager>();
            Terminal = UnityEngine.Object.FindFirstObjectByType<Terminal>();
            TimeOfDay = UnityEngine.Object.FindFirstObjectByType<TimeOfDay>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneLoaded += EventPatches.OnSceneLoaded;

            currentClientId = NetworkManager.Singleton.LocalClientId;

            //Removing the broken cardboard box item please understand 
            //Scrape Vanilla For Content References
            if (Plugin.IsSetupComplete == false)
            {
                StartOfRound.allItemsList.itemsList.RemoveAt(2);

                OnBeforeVanillaContentCollected.Invoke();

                DebugStopwatch.StartStopWatch("Scrape Vanilla Content");
                ContentExtractor.TryScrapeVanillaItems(StartOfRound);
                ContentExtractor.TryScrapeVanillaUnlockableItems(StartOfRound);
                ContentExtractor.TryScrapeVanillaContent(StartOfRound, RoundManager);
                ContentExtractor.ObtainSpecialItemReferences();

                OnAfterVanillaContentCollected.Invoke();
            }

            //Startup LethalLevelLoader's Network Manager Instance
            ExtendedNetworkManager.SpawnNetworkSingletons();

            //Add the facility's firstTimeDungeonAudio additionally to RoundManager's list to fix a base game bug.
            RoundManager.firstTimeDungeonAudios = RoundManager.firstTimeDungeonAudios.ToList().AddItem(RoundManager.firstTimeDungeonAudios[0]).ToArray();
            DebugStopwatch.StartStopWatch("Fix AudioSource Settings");
            //Disable Spatialization In All AudioSources To Fix Log Spam Bug.
            foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                audioSource.spatialize = false;

            playerCameras.Clear();
            foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (camera.targetTexture != null && camera.targetTexture.name == "PlayerScreen")
                    playerCameras.Add(camera, camera.farClipPlane);

            if (Plugin.IsSetupComplete == false)
            {
                //Terminal Specific Reference Setup
                TerminalManager.CacheTerminalReferences();

                LevelManager.InitializeShipAnimatorOverrideController();

                DungeonLoader.defaultKeyPrefab = RoundManager.keyPrefab;
                LevelLoader.defaultQuicksandPrefab = RoundManager.quicksandPrefab;

                DebugStopwatch.StartStopWatch("Create Vanilla ExtendedContent");
                DebugStopwatch.StartStopWatch("Initialize Custom ExtendedContent"); // this is not used

                Events.OnInitializeContent.Invoke();

                PatchedContent.PopulateContentDictionaries();

                foreach (WeatherEffect weatherEffect in TimeOfDay.effects)
                {
                    if (weatherEffect.effectObject != null && weatherEffect.effectObject.name == "DustStorm")
                        if (weatherEffect.effectObject.TryGetComponent(out LocalVolumetricFog dustFog))
                        {
                            LevelLoader.dustCloudFog = dustFog;
                            LevelLoader.defaultDustCloudFogVolumeSize = dustFog.parameters.size;
                            break;
                        }
                }

                LevelLoader.foggyFog = TimeOfDay.foggyWeather;
                LevelLoader.defaultFoggyFogVolumeSize = TimeOfDay.foggyWeather.parameters.size;

                foreach (ExtendedLevel vanillaLevel in PatchedContent.VanillaExtendedLevels)
                {
                    vanillaLevel.OverrideDustStormVolumeSize = LevelLoader.defaultDustCloudFogVolumeSize;
                    vanillaLevel.OverrideFoggyVolumeSize = LevelLoader.defaultFoggyFogVolumeSize;
                }
                foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                {
                    if (customLevel.OverrideDustStormVolumeSize == Vector3.zero)
                        customLevel.OverrideDustStormVolumeSize = LevelLoader.defaultDustCloudFogVolumeSize;
                    if (customLevel.OverrideFoggyVolumeSize == Vector3.zero)
                        customLevel.OverrideFoggyVolumeSize = LevelLoader.defaultFoggyFogVolumeSize;
                }

                //Some Debugging.
                string debugString = "LethalLevelLoader Loaded The Following ExtendedLevels:" + "\n";
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    debugString += (PatchedContent.ExtendedLevels.IndexOf(extendedLevel) + 1) + ". " + extendedLevel.SelectableLevel.PlanetName + " (" + extendedLevel.ContentType + ")" + "\n";
                DebugHelper.Log(debugString, DebugType.User);

                debugString = "LethalLevelLoader Loaded The Following ExtendedDungeonFlows:" + "\n";
                foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.ExtendedDungeonFlows)
                    debugString += (PatchedContent.ExtendedDungeonFlows.IndexOf(extendedDungeonFlow) + 1) + ". " + extendedDungeonFlow.DungeonName + " (" + extendedDungeonFlow.DungeonFlow.name + ") (" + extendedDungeonFlow.ContentType + ")" + "\n";
                DebugHelper.Log(debugString, DebugType.User);

                DebugStopwatch.StartStopWatch("Restore Content");
                //Restore Custom Content References To Vanilla Content
                foreach (ExtendedLevel customLevel in PatchedContent.CustomExtendedLevels)
                    ContentRestorer.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
                    ContentRestorer.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                //Destroy Placeholder Custom Content References That Have Now Been Restored
                ContentRestorer.DestroyRestoredAssets();

                OnAfterCustomContentRestored.Invoke();

                DebugStopwatch.StartStopWatch("Dynamic Risk Level");

                //Use Vanilla SelectableLevel's To Populate Information About Moon Difficulty.
                LevelManager.PopulateDynamicRiskLevelDictionary();

                //Assign Risk Level's To Custom SelectableLevel's Using The Populated Vanilla Information As Reference
                LevelManager.AssignCalculatedRiskLevels();

                DebugStopwatch.StartStopWatch("Apply, Merge & Populate Content Tags");

                //Apply ContentTags To Vanilla ExtendedContent Objects.
                ContentTagParser.ApplyVanillaContentTags();

                //Iterate Through All ExtendedMod Objects And Merge Any Reoccurring ContentTagName In The Same ExtendedMod.
                ContentTagManager.MergeAllExtendedModTags();

                //Populate Information About All Current ContentTag's Used In ExtendedContent For Developer Use.
                ContentTagManager.PopulateContentTagData();

                //Debugging.
                DebugHelper.DebugAllContentTags();
            }

            DebugStopwatch.StartStopWatch("Bind Configs");
            //Bind User Configuration Information.
            ConfigLoader.BindConfigs();

            DebugStopwatch.StartStopWatch("Patch Base game Lists");
            //Patch The Base game References To SelectableLevel's To Include Enabled Custom SelectableLevels.

            Events.OnPatchGame.Invoke();

            DebugStopwatch.StartStopWatch("ExtendedItem Injection");

            //Dynamically Inject Custom Item's Into SelectableLevel's Based On Level & Dungeon MatchingProperties.
            ItemManager.RefreshDynamicItemRarityOnAllExtendedLevels();

            DebugStopwatch.StartStopWatch("ExtendedEnemyType Injection");

            //Dynamically Inject Custom EnemyType's Into SelectableLevel's Based On Level & Dungeon MatchingProperties.
            EnemyManager.RefreshDynamicEnemyTypeRarityOnAllExtendedLevels();

            DebugStopwatch.StartStopWatch("ExtendedBuyableVehicle Injection");

            DebugStopwatch.StartStopWatch("ExtendedUnlockableItem Injection");

            DebugStopwatch.StartStopWatch("Create ExtendedLevelGroups & Filter Assets");

            //Populate SelectableLevel Data To Be Used In Overhaul Of The Terminal Moons Catalogue.
            TerminalManager.CreateExtendedLevelGroups();

            if (Plugin.IsSetupComplete == false)
            {
                //Populate SelectableLevel Data To Be Used In Overhaul Of The Terminal Moons Catalogue.
                TerminalManager.CreateMoonsFilterTerminalAssets();

                foreach (CompatibleNoun routeNode in TerminalManager.Keyword_Route.compatibleNouns)
                    TerminalManager.AddTerminalNodeEventListener(routeNode.result, TerminalManager.OnBeforeRouteNodeLoaded, TerminalManager.LoadNodeActionType.Before);

                TerminalManager.AddTerminalNodeEventListener(TerminalManager.Keyword_Moons.specialKeywordResult, TerminalManager.RefreshMoonsCataloguePage, TerminalManager.LoadNodeActionType.After);
            }

            LevelLoader.defaultFootstepSurfaces = new List<FootstepSurface>(StartOfRound.footstepSurfaces).ToArray();

            DebugStopwatch.StartStopWatch("Initialize Save");

            if (ExtendedNetworkManager.NetworkManagerInstance.IsServer)
                SaveManager.InitializeSave();

            DebugStopwatch.StopStopWatch("Initialize Save");
            if (Plugin.IsSetupComplete == false)
            {
                Plugin.CompleteSetup();
                StartOfRound.SetPlanetsWeather();
            }
            Plugin.LobbyInitialized();
        }

        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static bool StartOfRoundSetPlanetsWeather_Prefix(int connectedPlayersOnServer)
        {
            if (Plugin.IsSetupComplete == false)
            {
                DebugHelper.LogWarning("Exiting SetPlanetsWeather() Early To Avoid Weather Being Set Before Custom Levels Are Registered.", DebugType.User);
                return (false);
            }
            return (true);
        }

        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather"), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void StartOfRoundSetPlanetsWeather_Postfix()
        {
            if (IsServer)
                ExtendedNetworkManager.InvokeWhenInitalized(ExtendedNetworkManager.TryRefreshWeather);
        }

        public static bool hasInitiallyChangedLevel;
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel"), HarmonyPrefix, HarmonyPriority(priority)]
        public static bool StartOfRoundChangeLevel_Prefix(ref int levelID)
        {
            if (ExtendedNetworkManager.NetworkManagerInstance.IsServer == false) return (true);

            //Because Level ID's can change between modpack adjustments and such, we save the name of the level instead and find and load that up instead of the saved ID the base game uses.
            if (hasInitiallyChangedLevel == false && !string.IsNullOrEmpty(SaveManager.currentSaveFile.CurrentLevelName))
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.SelectableLevel.name == SaveManager.currentSaveFile.CurrentLevelName)
                    {
                        DebugHelper.Log("Loading Previously Saved SelectableLevel: " + extendedLevel.SelectableLevel.PlanetName, DebugType.User);
                        levelID = StartOfRound.levels.ToList().IndexOf(extendedLevel.SelectableLevel);
                        hasInitiallyChangedLevel = true;
                        return (true);
                    }


            //If we can't find the previous current level, that probably means the game is going to try and use an ID bigger than the current array, or reference the wrong level, so we reset it back to experimentation here.
            if (hasInitiallyChangedLevel == false && !string.IsNullOrEmpty(SaveManager.currentSaveFile.CurrentLevelName) && !SaveManager.currentSaveFile.CurrentLevelName.Contains("Experimentation") && (levelID >= StartOfRound.levels.Length || levelID > OriginalContent.SelectableLevels.Count))
                levelID = 0;

            hasInitiallyChangedLevel = true;
            return (true);
        }


        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel"), HarmonyPostfix, HarmonyPriority(priority)]
        public static void StartOfRoundChangeLevel_Postfix(int levelID)
        {
            NetworkBundleManager.TryRefresh();
            if (IsServer && RoundManager.currentLevel != null && SaveManager.currentSaveFile.CurrentLevelName != RoundManager.currentLevel.PlanetName)
            {
                DebugHelper.Log("Saving Current SelectableLevel: " + RoundManager.currentLevel.PlanetName, DebugType.User);
                SaveManager.currentSaveFile.CurrentLevelName = RoundManager.currentLevel.name;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "LoadShipGrabbableItems"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void StartOfRoundLoadShipGrabbableItems_Prefix()
        {
            SaveManager.LoadShipGrabbableItems();
        }

        [HarmonyPatch(typeof(Terminal), "ParseWord"), HarmonyPostfix, HarmonyPriority(priority)]
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

        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static bool TerminalRunTerminalEvents_Prefix(Terminal __instance, TerminalNode node)
        {
            return (TerminalManager.OnBeforeLoadNewNode(ref node));
        }

        [HarmonyPatch(typeof(Terminal), "LoadNewNode"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static bool TerminalLoadNewNode_Prefix(Terminal __instance, ref TerminalNode node)
        {
            Terminal.screenText.textComponent.fontSize = TerminalManager.defaultTerminalFontSize;
            return (TerminalManager.OnBeforeLoadNewNode(ref node));
        }

        [HarmonyPatch(typeof(Terminal), "LoadNewNode"), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void TerminalLoadNewNode_Postfix(Terminal __instance, ref TerminalNode node)
        {
            TerminalManager.OnLoadNewNode(ref node);
        }

        //Called via SceneManager event.
        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Events.TryUpdateGameState(scene.name);
            if (Events.CurrentState != GameStates.Moon)
                Events.SetLobbyState(false);
            if (LevelManager.CurrentExtendedLevel == null || LevelManager.CurrentExtendedLevel.IsLevelLoaded == false) return;
            foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.SelectableLevel.sceneName).GetRootGameObjects())
            {
                LevelLoader.RefreshFogSize(LevelManager.CurrentExtendedLevel);
                ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "StartGame"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void StartOfRoundStartGame_Prefix()
        {
            ExtendedLevel extendedLevel = LevelManager.CurrentExtendedLevel;
            if (!IsServer || extendedLevel == null) return;

            extendedLevel.SelectableLevel.sceneName = string.Empty;
            RoundManager.InitializeRandomNumberGenerators();

            int counter = 1;
            foreach (StringWithRarity sceneSelection in extendedLevel.SceneSelections)
            {
                DebugHelper.Log("Scene Selection #" + counter + " \"" + sceneSelection.Name + "\" (" + sceneSelection.Rarity + ")", DebugType.Developer);
                counter++;
            }

            List<int> sceneSelections = extendedLevel.SceneSelections.Select(s => s.Rarity).ToList();
            int selectedSceneIndex = RoundManager.GetRandomWeightedIndex(sceneSelections.ToArray(), RoundManager.LevelRandom);
            extendedLevel.SelectableLevel.sceneName = extendedLevel.SceneSelections[selectedSceneIndex].Name;
            DebugHelper.Log("Selected SceneName: " + extendedLevel.SelectableLevel.sceneName + " For ExtendedLevel: " + extendedLevel.NumberlessPlanetName, DebugType.Developer);
        }

        [HarmonyPatch(typeof(DungeonGenerator), "Generate"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (LevelManager.CurrentExtendedLevel != null)
                DungeonLoader.PrepareDungeon();
            LevelManager.LogDayHistory();

            if (Patches.RoundManager.dungeonGenerator.Generator.DungeonFlow == null)
                DebugHelper.LogError("Critical Failure! DungeonGenerator DungeonFlow Is Null!", DebugType.User);
        }

        //Base game has a bug where it stops listening before it gets the Complete call, so this is just a fixed version of the base game function.
        [HarmonyPatch(typeof(RoundManager), "Generator_OnGenerationStatusChanged"), HarmonyPrefix, HarmonyPriority(priority)]
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
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenerateNewLevelClientRpcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .SearchForward(instructions => instructions.Calls(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(InjectHostDungeonFlowSelection))))
                .Advance(-1)
                .SetInstruction(new CodeInstruction(OpCodes.Nop));
            return (codeMatcher.InstructionEnumeration());
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor)), HarmonyTranspiler]
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
            roundManager.dungeonGenerator.Generate();
        }

        //Called via Transpiler.
        internal static void InjectHostDungeonFlowSelection()
        {
            if (LevelManager.CurrentExtendedLevel != null)
                DungeonLoader.SelectDungeon();
            else
                Patches.RoundManager.GenerateNewFloor();
        }

        [HarmonyPatch(typeof(RoundManager), "SetLockedDoors"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void RoundManagerSetLockedDoors_Prefix()
        {
            RoundManager.keyPrefab = DungeonManager.CurrentExtendedDungeonFlow.OverrideKeyPrefab != null ? DungeonManager.CurrentExtendedDungeonFlow.OverrideKeyPrefab : DungeonLoader.defaultKeyPrefab;
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void RoundManagerSpawnOutsideHazards_Prefix()
        {
            RoundManager.quicksandPrefab = LevelManager.CurrentExtendedLevel.OverrideQuicksandPrefab;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void RoundManagerFinishGeneratingNewLevelClientRpc_Prefix()
        {
            if (TimeOfDay.sunAnimator == null) return;
            LevelLoader.RefreshFootstepSurfaces();
            LevelLoader.BakeSceneColliderMaterialData(TimeOfDay.sunAnimator.gameObject.scene);
            if (LevelLoader.vanillaWaterShader != null)
                LevelLoader.TryRestoreWaterShaders(TimeOfDay.sunAnimator.gameObject.scene);
            ApplyCamerDistanceOverride();
        }

        internal static void ApplyCamerDistanceOverride()
        {
            float newDistance = 0;
            if (LevelManager.CurrentExtendedLevel.OverrideCameraMaxDistance > 400f || (DungeonManager.CurrentExtendedDungeonFlow != null && DungeonManager.CurrentExtendedDungeonFlow.OverrideCameraMaxDistance > 400f))
                newDistance = Mathf.Max(LevelManager.CurrentExtendedLevel.OverrideCameraMaxDistance, DungeonManager.CurrentExtendedDungeonFlow.OverrideCameraMaxDistance);
            foreach (KeyValuePair<Camera, float> cameraPair in playerCameras)
                cameraPair.Key.farClipPlane = Mathf.Max(cameraPair.Value, newDistance);
        }

        [HarmonyPatch(typeof(StoryLog), "Start"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void StoryLogStart_Prefix(StoryLog __instance)
        {
            foreach (ExtendedStoryLog extendedStoryLog in LevelManager.CurrentExtendedLevel.ExtendedMod.ExtendedStoryLogs)
                if (extendedStoryLog.sceneName == __instance.gameObject.scene.name)
                {
                    if (__instance.storyLogID == extendedStoryLog.storyLogID)
                    {
                        DebugHelper.Log("Updating " + extendedStoryLog.storyLogTitle + "ID", DebugType.Developer);
                        __instance.storyLogID = extendedStoryLog.StoryLogID;
                    }
                }
        }

        static List<SpawnableMapObject> temporarySpawnableMapObjectList = new List<SpawnableMapObject>();
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects"), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void RoundManagerSpawnMapObjects_Prefix()
        {
            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);
            foreach (SpawnableMapObject newRandomMapObject in DungeonManager.CurrentExtendedDungeonFlow.SpawnableMapObjects)
            {
                spawnableMapObjects.Add(newRandomMapObject);
                temporarySpawnableMapObjectList.Add(newRandomMapObject);
            }
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects"), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void RoundManagerSpawnMapObjects_Postfix()
        {
            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);
            foreach (SpawnableMapObject spawnableMapObject in temporarySpawnableMapObjectList)
                spawnableMapObjects.Remove(spawnableMapObject);
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();
            temporarySpawnableMapObjectList.Clear();
        }

        static FootstepSurface previousFootstepSurface;

        [HarmonyPatch(typeof(PlayerControllerB), "GetCurrentMaterialStandingOn"), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void PlayerControllerBGetCurrentMaterialStandingOn_Postfix(PlayerControllerB __instance)
        {
            if (LevelLoader.TryGetFootstepSurface(__instance.hit.collider, out FootstepSurface footstepSurface))
                __instance.currentFootstepSurfaceIndex = StartOfRound.footstepSurfaces.IndexOf(footstepSurface);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientConnect)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void StartOfRoundOnClientConnect_Postfix()
        {
            NetworkBundleManager.Instance.OnClientsChangedRefresh();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientDisconnect)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void StartOfRoundOnClientDisconnect_Postfix(ulong clientId)
        {
            if (clientId != currentClientId)
                NetworkBundleManager.Instance.OnClientsChangedRefresh();
        }

        [HarmonyPatch(typeof(NetworkConnectionManager), nameof(NetworkConnectionManager.OnClientDisconnectFromServer)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void NetworkConnectionManagerOnClientDisconnectFromServer_Postfix(ulong clientId)
        {
            if (clientId != currentClientId)
                NetworkBundleManager.Instance.OnClientsChangedRefresh();
        }

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Start)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void StartMatchLeverStart_Postfix(StartMatchLever __instance)
        {
            previousHoverTip = __instance.triggerScript.hoverTip;
            previousInteractableState = __instance.triggerScript.interactable;
        }

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Update)), HarmonyPrefix, HarmonyPriority(priority)]
        internal static void StartMatchLeverUpdate_Prefix(StartMatchLever __instance)
        {
            if (SceneManager.loadedSceneCount > 1) return;
            __instance.triggerScript.disabledHoverTip = NetworkBundleManager.AllowedToLoadLevel ? previousHoverTip : disabledText;
            __instance.triggerScript.interactable = NetworkBundleManager.AllowedToLoadLevel ? previousInteractableState : false;

            if (NetworkBundleManager.AllowedToLoadLevel == true)
            {
                previousInteractableState = __instance.triggerScript.interactable;
                previousHoverTip = __instance.triggerScript.disabledHoverTip;
            }
        }


        internal const string disabledText = "[ At least one player is loading custom moon! ]";
        private static string previousHoverTip;
        private static bool previousInteractableState;
        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Update)), HarmonyPostfix, HarmonyPriority(priority)]
        internal static void StartMatchLeverUpdate_Postfix(StartMatchLever __instance)
        {
            if (SceneManager.loadedSceneCount > 1) return;
            __instance.triggerScript.disabledHoverTip = NetworkBundleManager.AllowedToLoadLevel ? previousHoverTip : disabledText;
            __instance.triggerScript.interactable = NetworkBundleManager.AllowedToLoadLevel ? previousInteractableState : false;
        }

        //DunGen Optimization Patches (Credit To LadyRaphtalia, Author Of Scarlet Devil Mansion)
        [HarmonyPatch(typeof(DoorwayPairFinder), "GetDoorwayPairs"), HarmonyPrefix, HarmonyPriority(priority)]
        public static bool GetDoorwayPairsPatch(ref DoorwayPairFinder __instance, int? maxCount, ref Queue<DoorwayPair> __result)
        {

            __instance.tileOrder = __instance.CalculateOrderedListOfTiles();
            var doorwayPairs = __instance.PreviousTile == null ?
              __instance.GetPotentialDoorwayPairsForFirstTile() :
              __instance.GetPotentialDoorwayPairsForNonFirstTile();

            var num = doorwayPairs.Count();
            if (maxCount != null)
                num = Mathf.Min(num, maxCount.Value);
            __result = new Queue<DoorwayPair>(num);

            var newList = OrderDoorwayPairs(doorwayPairs, num);
            foreach (var item in newList)
                __result.Enqueue(item);

            return false;
        }

        private class DoorwayPairComparer : IComparer<DoorwayPair>
        {
            public int Compare(DoorwayPair x, DoorwayPair y)
            {
                var tileWeight = y.TileWeight.CompareTo(x.TileWeight);
                if (tileWeight == 0) return y.DoorwayWeight.CompareTo(x.DoorwayWeight);
                return tileWeight;
            }
        }

        private static IEnumerable<DoorwayPair> OrderDoorwayPairs(IEnumerable<DoorwayPair> list, int num)
        {
            return list.OrderBy(x => x, new DoorwayPairComparer()).Take(num);
        }

        //IL Hook stuff to replace Mold related save data references to a Level's ID to instead the Level's Name. Credit to Hamunii.
        private static readonly HookHelper.DisposableHookCollection monomodHooks = new();
        internal static void InitMonoModHooks()
        {
            monomodHooks.ILHook<GameNetworkManager>(nameof(GameNetworkManager.SaveGameValues), ReplaceSavedMoldLevelIDsWithLevelNames_ILHook);
            monomodHooks.ILHook<GameNetworkManager>(nameof(GameNetworkManager.ResetSavedGameValues), ReplaceSavedMoldLevelIDsWithLevelNames_ILHook);
            monomodHooks.ILHook<StartOfRound>(nameof(StartOfRound.LoadPlanetsMoldSpreadData), ReplaceSavedMoldLevelIDsWithLevelNames_ILHook);
            monomodHooks.ILHook<MoldSpreadManager>(nameof(MoldSpreadManager.Start), ReplaceSavedMoldLevelIDsWithLevelNames_ILHook);
        }

        private static void ReplaceSavedMoldLevelIDsWithLevelNames_ILHook(ILContext il)
        {
            int dbgModificationsAmount = 0;
            string dbgMatchedStr = "";

            ILCursor c = new(il);
            while (
                c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr(out dbgMatchedStr) && dbgMatchedStr.Contains("Mold"), // The save file key, e.g. "Level{0}Mold"
                    x => true   // game has various ways of referencing StartOfRound, listed here purely for reference:
                        || x.MatchLdloc(out _)                                                  // via local variable
                        || x.MatchLdarg(0)                                                      // via 'this'
                        || x.MatchCall<StartOfRound>("get_" + nameof(StartOfRound.Instance)),   // via StartOfRound.Instance
                    x => x.MatchLdfld<StartOfRound>(nameof(StartOfRound.levels)),
                    x => x.MatchLdloc(out _),
                    x => x.MatchLdelemRef(),
                    x => x.MatchLdfld<SelectableLevel>(nameof(SelectableLevel.levelID)),
                    x => x.MatchBox<Int32>()
                )
            )
            {
                c.Index -= 2;
                c.RemoveRange(2);
                c.EmitDelegate<Func<SelectableLevel, object>>(selectableLevel =>
                    { return selectableLevel.name; }
                );
                dbgModificationsAmount++;
            }
            DebugHelper.Log($"Modified {dbgModificationsAmount} save data level IDs to level names", DebugType.Developer);
        }
    }
}