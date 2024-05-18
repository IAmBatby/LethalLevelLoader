using Discord;
using DunGen;
using DunGen.Graph;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Device;
using UnityEngine.SceneManagement;
using static LethalLevelLoader.AssetBundleLoader;
using NetworkManager = Unity.Netcode.NetworkManager;

namespace LethalLevelLoader
{
    internal static class Patches
    {
        internal const int harmonyPriority = 200;

        internal static string delayedSceneLoadingName = string.Empty;

        internal static List<string> allSceneNamesCalledToLoad = new List<string>();

        //Singletons and such for these are set in each classes Awake function, But they all are accessible on the first awake function of the earliest one of these four managers awake function, so i grab them directly via findobjectoftype to safely access them as early as possible.
        public static StartOfRound StartOfRound { get; internal set; }
        public static RoundManager RoundManager { get; internal set; }
        public static Terminal Terminal { get; internal set; }
        public static TimeOfDay TimeOfDay { get; internal set; }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static void PreInitSceneScriptAwake_Prefix(PreInitSceneScript __instance)
        {
            if (Plugin.IsSetupComplete == false)
            {
                //AssetBundleLoader.CreateLoadingBundlesHeaderText(__instance);
                if (__instance.TryGetComponent(out AudioSource audioSource))
                    OriginalContent.AudioMixers.Add(audioSource.outputAudioMixerGroup.audioMixer);

                //AssetBundleLoader.LoadBundles(__instance);
                //AssetBundleLoader.onBundlesFinishedLoading += AssetBundleLoader.LoadContentInBundles;

                if (AssetBundleLoader.noBundlesFound == true)
                {
                    CurrentLoadingStatus = LoadingStatus.Complete;
                    AssetBundleLoader.OnBundlesFinishedLoadingInvoke();
                }


                ContentTagParser.ImportVanillaContentTags();
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

            if (AssetBundleLoader.CurrentLoadingStatus == AssetBundleLoader.LoadingStatus.Loading)
            {
                DebugHelper.LogWarning("SceneManager has attempted to load " + sceneName + " Scene before AssetBundles have finished loading. Pausing request until LethalLeveLoader is ready to proceed.", DebugType.User);
                delayedSceneLoadingName = sceneName;
                AssetBundleLoader.onBundlesFinishedLoading -= LoadMainMenu;
                AssetBundleLoader.onBundlesFinishedLoading += LoadMainMenu;

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

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_Prefix(GameNetworkManager __instance)
        {
            if (Plugin.IsSetupComplete == false)
            {
                LethalLevelLoaderNetworkManager.networkManager = __instance.GetComponent<NetworkManager>();
                foreach (NetworkPrefab networkPrefab in __instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.Prefabs)
                    if (networkPrefab.Prefab.name.Contains("EntranceTeleport"))
                        if (networkPrefab.Prefab.GetComponent<AudioSource>() != null)
                            OriginalContent.AudioMixers.Add(networkPrefab.Prefab.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                GameObject networkManagerPrefab = PrefabHelper.CreateNetworkPrefab("LethalLevelLoaderNetworkManagerTest");
                networkManagerPrefab.AddComponent<LethalLevelLoaderNetworkManager>();
                networkManagerPrefab.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
                networkManagerPrefab.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
                networkManagerPrefab.GetComponent<NetworkObject>().DestroyWithScene = false;
                GameObject.DontDestroyOnLoad(networkManagerPrefab);
                LethalLevelLoaderNetworkManager.networkingManagerPrefab = networkManagerPrefab;
                LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(networkManagerPrefab);

                AssetBundleLoader.NetworkRegisterCustomContent(__instance.GetComponent<NetworkManager>());
                LethalLevelLoaderNetworkManager.RegisterPrefabs(__instance.GetComponent<NetworkManager>());
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPrefix]
        internal static void StartOfRoundAwake_Prefix(StartOfRound __instance)
        {
            Plugin.OnBeforeSetupInvoke();
            //Reference Setup
            StartOfRound = __instance;
            //StartOfRound.Instance = __instance;
            RoundManager = UnityEngine.Object.FindFirstObjectByType<RoundManager>();
            //RoundManager.Instance = RoundManager;
            Terminal = UnityEngine.Object.FindFirstObjectByType<Terminal>();
            TimeOfDay = UnityEngine.Object.FindFirstObjectByType<TimeOfDay>();
            //TimeOfDay.Instance = TimeOfDay;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneLoaded += EventPatches.OnSceneLoaded;

            //Removing the broken cardboard box item please understand 

            //Scrape Vanilla For Content References
            if (Plugin.IsSetupComplete == false)
            {
                StartOfRound.allItemsList.itemsList.RemoveAt(2);
                SaveManager.defaultCachedItemsList = new List<Item>(StartOfRound.allItemsList.itemsList);

                DebugStopwatch.StartStopWatch("Scrape Vanilla Content");
                ContentExtractor.TryScrapeVanillaItems(StartOfRound);
                ContentExtractor.TryScrapeVanillaContent(StartOfRound, RoundManager);
                ContentExtractor.ObtainSpecialItemReferences();
            }

            //Startup LethalLevelLoader's Network Manager Instance
            if (GameNetworkManager.Instance.GetComponent<NetworkManager>().IsServer)
                GameObject.Instantiate(LethalLevelLoaderNetworkManager.networkingManagerPrefab).GetComponent<NetworkObject>().Spawn(destroyWithScene: false);

            //Add the facility's firstTimeDungeonAudio additionally to RoundManager's list to fix a basegame bug.
            RoundManager.firstTimeDungeonAudios = RoundManager.firstTimeDungeonAudios.ToList().AddItem(RoundManager.firstTimeDungeonAudios[0]).ToArray();
            DebugStopwatch.StartStopWatch("Fix AudioSource Settings");
            //Disable Spatialization In All AudioSources To Fix Log Spam Bug.
            foreach (AudioSource audioSource in Resources.FindObjectsOfTypeAll<AudioSource>())
                audioSource.spatialize = false;

            if (Plugin.IsSetupComplete == false)
            {
                //Terminal Specific Reference Setup
                TerminalManager.CacheTerminalReferences();

                LevelManager.InitalizeShipAnimatorOverrideController();

                DungeonLoader.defaultKeyPrefab = RoundManager.keyPrefab;
                LevelLoader.defaultQuicksandPrefab = RoundManager.quicksandPrefab;

                DebugStopwatch.StartStopWatch("Create Vanilla ExtendedContent");
                //Create & Initialize ExtendedContent Objects For Vanilla Content.
                AssetBundleLoader.CreateVanillaExtendedDungeonFlows();
                AssetBundleLoader.CreateVanillaExtendedLevels(StartOfRound);
                AssetBundleLoader.CreateVanillaExtendedItems();
                AssetBundleLoader.CreateVanillaExtendedEnemyTypes();

                DebugStopwatch.StartStopWatch("Initalize Custom ExtendedContent");
                //Initialize ExtendedContent Objects For Custom Content.
                AssetBundleLoader.InitializeBundles();

                foreach (ExtendedLevel extendedLevel in PatchedContent.CustomExtendedLevels)
                    extendedLevel.SetLevelID();

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

                DebugStopwatch.StartStopWatch("Dynamic Risk Level");

                //Use Vanilla SelectableLevel's To Populate Information About Moon Difficulty.
                LevelManager.PopulateDynamicRiskLevelDictionary();

                //Assign Risk Level's To Custom SelectableLevel's Using The Populated Vanilla Information As Reference
                LevelManager.AssignCalculatedRiskLevels();

                DebugStopwatch.StartStopWatch("Apply, Merge & Populate Content Tags");

                //Apply ContentTags To Vanilla ExtendedContent Objects.
                ContentTagParser.ApplyVanillaContentTags();

                //Iterate Through All ExtendedMod Objects And Merge Any Reoccuring ContentTagName In The Same ExtendedMod.
                ContentTagManager.MergeAllExtendedModTags();

                //Populate Information About All Current ContentTag's Used In ExtendedContent For Developer Use.
                ContentTagManager.PopulateContentTagData();

                //Debugging.
                DebugHelper.DebugAllContentTags();
                ItemManager.GetExtendedItemPriceData();
                ItemManager.GetExtendedItemWeightData();
            }

            DebugStopwatch.StartStopWatch("Bind Configs");
            //Bind User Configation Information.
            ConfigLoader.BindConfigs();

            DebugStopwatch.StartStopWatch("Patch Basegame Lists");
            //Patch The Basegame References To SelectableLevel's To Include Enabled Custom SelectableLevels.
            LevelManager.PatchVanillaLevelLists();

            //Patch The Basegame References To DungeonFlows's To Include Enabled Custom DungeonFlows.
            DungeonManager.PatchVanillaDungeonLists();

            //Patch The Basegame References To EnemyTypes's To Include Enabled Custom EnemyTypes.
            EnemyManager.UpdateEnemyIDs(); //Might only need to do once?

            foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.CustomExtendedEnemyTypes)
                TerminalManager.CreateEnemyTypeTerminalData(extendedEnemyType); //Might only need to do once?

            EnemyManager.AddCustomEnemyTypesToTestAllEnemiesLevel(); //Might only need to do once?

            DebugStopwatch.StartStopWatch("ExtendedItem Injection");

            //Dynamically Inject Custom Item's Into SelectableLevel's Based On Level & Dungeon MatchingProperties.
            ItemManager.RefreshDynamicItemRarityOnAllExtendedLevels();

            DebugStopwatch.StartStopWatch("ExtendedEnemyType Injection");

            //Dynamically Inject Custom EnemyType's Into SelectableLevel's Based On Level & Dungeon MatchingProperties.
            EnemyManager.RefreshDynamicEnemyTypeRarityOnAllExtendedLevels();

            DebugStopwatch.StartStopWatch("Create ExtendedLevelGroups & Filter Assets");

            //Populate SelectableLevel Data To Be Used In Overhaul Of The Terminal Moons Catalogue.
            TerminalManager.CreateExtendedLevelGroups();

            if (Plugin.IsSetupComplete == false)
            {
                //Populate SelectableLevel Data To Be Used In Overhaul Of The Terminal Moons Catalogue.
                TerminalManager.CreateMoonsFilterTerminalAssets();

                foreach (CompatibleNoun routeNode in TerminalManager.routeKeyword.compatibleNouns)
                    TerminalManager.AddTerminalNodeEventListener(routeNode.result, TerminalManager.OnBeforeRouteNodeLoaded, TerminalManager.LoadNodeActionType.Before);

                //Create Terminal Data For Custom StoryLog's And Patch Basegame References To StoryLog's To Include Custom StoryLogs.
                TerminalManager.CreateTerminalDataForAllExtendedStoryLogs();

                TerminalManager.AddTerminalNodeEventListener(TerminalManager.moonsKeyword.specialKeywordResult, TerminalManager.RefreshMoonsCataloguePage, TerminalManager.LoadNodeActionType.After);
            }

            //We Might Not Need This Now
            /*if (LevelManager.invalidSaveLevelID != -1 && StartOfRound.levels.Length > LevelManager.invalidSaveLevelID)
            {
                DebugHelper.Log("Setting CurrentLevel to previously saved ID that was not loaded at the time of save loading.");
                DebugHelper.Log(LevelManager.invalidSaveLevelID + " / " + (StartOfRound.levels.Length));
                StartOfRound.ChangeLevelServerRpc(LevelManager.invalidSaveLevelID, TerminalManager.Terminal.groupCredits);
                LevelManager.invalidSaveLevelID = -1;
            }*/

            DebugStopwatch.StartStopWatch("Initalize Save");

            if (LethalLevelLoaderNetworkManager.networkManager.IsServer)
            {
                SaveManager.InitializeSave();
                SaveManager.RefreshSaveItemInfo();
            }

            DebugStopwatch.StopStopWatch("Initalize Save");
            if (Plugin.IsSetupComplete == false)
            {
                AssetBundleLoader.CreateVanillaExtendedWeatherEffects(StartOfRound, TimeOfDay);
                WeatherManager.PopulateVanillaExtendedWeatherEffectsDictionary();
                WeatherManager.PopulateExtendedLevelEnabledExtendedWeatherEffects();
                Plugin.CompleteSetup();
                StartOfRound.SetPlanetsWeather();
            }
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
        [HarmonyPrefix]
        internal static bool StartOfRoundSetPlanetsWeather_Prefix(int connectedPlayersOnServer)
        {
            if (Plugin.IsSetupComplete == false)
            {
                DebugHelper.LogWarning("Exiting SetPlanetsWeather() Early To Avoid Weather Being Set Before Custom Levels Are Registered.", DebugType.User);
                return (false);
            }
            return (true);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
        [HarmonyPostfix]
        internal static void StartOfRoundSetPlanetsWeather_Postfix()
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer)
                LethalLevelLoaderNetworkManager.Instance.GetUpdatedLevelCurrentWeatherServerRpc();
        }

        public static bool hasInitiallyChangedLevel;
        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
        [HarmonyPrefix]
        public static bool StartOfRoundChangeLevel_Prefix(ref int levelID)
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return (true);

            //Because Level ID's can change between modpack adjustments and such, we save the name of the level instead and find and load that up instead of the saved ID the basegame uses.
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


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
        [HarmonyPostfix]
        public static void StartOfRoundChangeLevel_Postfix(int levelID)
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;

            if (RoundManager.currentLevel != null && SaveManager.currentSaveFile.CurrentLevelName != RoundManager.currentLevel.PlanetName)
            {
                DebugHelper.Log("Saving Current SelectableLevel: " + RoundManager.currentLevel.PlanetName, DebugType.User);
                SaveManager.SaveCurrentSelectableLevel(RoundManager.currentLevel);
                //LevelLoader.RefreshShipAnimatorClips(LevelManager.CurrentExtendedLevel);
            }
            
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "LoadShipGrabbableItems")]
        [HarmonyPrefix]
        internal static bool StartOfRoundLoadShipGrabbableItems_Prefix()
        {
            //SaveManager.LoadShipGrabbableItems(); 
            //return (false);
            return (true);
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
            /*
            bool returnBool = true;
            if (node.terminalEvent.Contains("simulate"))
            {
                ranLethalLevelLoaderTerminalEvent = false;
                //TerminalManager.SetSimulationResultsText(node);
                returnBool = true;
            }
            else if (__instance.currentNode != TerminalManager.moonsKeyword.specialKeywordResult)
            {
                ranLethalLevelLoaderTerminalEvent = false;
                returnBool = true;
            }
            else
            {
                ranLethalLevelLoaderTerminalEvent = !TerminalManager.RunLethalLevelLoaderTerminalEvents(node);
                returnBool = !ranLethalLevelLoaderTerminalEvent;
            }

            returnBool = TerminalManager.OnBeforeLoadNewNode(node);

            return (returnBool);*/

            return (TerminalManager.OnBeforeLoadNewNode(ref node));
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPrefix]
        internal static bool TerminalLoadNewNode_Prefix(Terminal __instance, ref TerminalNode node)
        {
            Terminal.screenText.textComponent.fontSize = TerminalManager.defaultTerminalFontSize;
            return (TerminalManager.OnBeforeLoadNewNode(ref node));
                /*foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (extendedLevel.RouteNode == node && extendedLevel.IsRouteLocked == true)
                        TerminalManager.SwapRouteNodeToLockedNode(extendedLevel, ref node);*/
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPostfix]
        internal static void TerminalLoadNewNode_Postfix(Terminal __instance, ref TerminalNode node)
        {
            TerminalManager.OnLoadNewNode(ref node);
            //if (ranLethalLevelLoaderTerminalEvent == true)
                //__instance.currentNode = TerminalManager.moonsKeyword.specialKeywordResult;
        }

        //Called via SceneManager event.
        internal static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (LevelManager.CurrentExtendedLevel != null && LevelManager.CurrentExtendedLevel.IsLevelLoaded)
                foreach (GameObject rootObject in SceneManager.GetSceneByName(LevelManager.CurrentExtendedLevel.SelectableLevel.sceneName).GetRootGameObjects())
                {
                    LevelLoader.UpdateStoryLogs(LevelManager.CurrentExtendedLevel, rootObject);
                    ContentRestorer.RestoreAudioAssetReferencesInParent(rootObject);
                }

        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPrefix]
        internal static void StartOfRoundStartGame_Prefix()
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;

            ExtendedLevel extendedLevel = LevelManager.CurrentExtendedLevel;

            if (extendedLevel == null) return;

            extendedLevel.SelectableLevel.sceneName = string.Empty;

            RoundManager.InitializeRandomNumberGenerators();

            int counter = 1;
            foreach (StringWithRarity sceneSelection in LevelManager.CurrentExtendedLevel.SceneSelections)
            {
                DebugHelper.Log("Scene Selection #" + counter + " \"" + sceneSelection.Name + "\" (" + sceneSelection.Rarity + ")", DebugType.Developer);
                counter++;
            }

            List<int> sceneSelections = LevelManager.CurrentExtendedLevel.SceneSelections.Select(s => s.Rarity).ToList();
            int selectedSceneIndex = RoundManager.GetRandomWeightedIndex(sceneSelections.ToArray(), RoundManager.LevelRandom);
            extendedLevel.SelectableLevel.sceneName = LevelManager.CurrentExtendedLevel.SceneSelections[selectedSceneIndex].Name;
            DebugHelper.Log("Selected SceneName: " + extendedLevel.SelectableLevel.sceneName + " For ExtendedLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName, DebugType.Developer);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        internal static void DungeonGeneratorGenerate_Prefix(DungeonGenerator __instance)
        {
            if (LevelManager.CurrentExtendedLevel != null)
                DungeonLoader.PrepareDungeon();
            LevelManager.LogDayHistory();

            if (Patches.RoundManager.dungeonGenerator.Generator.DungeonFlow == null)
                DebugHelper.LogError("Critical Failure! DungeonGenerator DungeonFlow Is Null!", DebugType.User);
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
            /*if (LevelManager.CurrentExtendedLevel != null)
                LethalLevelLoaderNetworkManager.Instance.GetDungeonFlowSizeServerRpc();
            else*/
                roundManager.dungeonGenerator.Generate();
        }

        //Called via Transpiler.
        internal static void InjectHostDungeonFlowSelection()
        {
            if (LevelManager.CurrentExtendedLevel != null)
            {
                //DungeonManager.TryAddCurrentVanillaLevelDungeonFlow(Patches.RoundManager.dungeonGenerator.Generator, LevelManager.CurrentExtendedLevel);
                DungeonLoader.SelectDungeon();
            }
            else
                Patches.RoundManager.GenerateNewFloor();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SetLockedDoors")]
        [HarmonyPrefix]
        internal static void RoundManagerSetLockedDoors_Prefix()
        {
            RoundManager.keyPrefab = DungeonManager.CurrentExtendedDungeonFlow.OverrideKeyPrefab;
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards")]
        [HarmonyPrefix]
        internal static void RoundManagerSpawnOutsideHazards_Prefix()
        {
            RoundManager.quicksandPrefab = LevelManager.CurrentExtendedLevel.OverrideQuicksandPrefab;
        }


        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(StoryLog), "Start")]
        [HarmonyPrefix]
        internal static void StoryLogStart_Prefix(StoryLog __instance)
        {
            foreach (ExtendedStoryLog extendedStoryLog in LevelManager.CurrentExtendedLevel.ExtendedMod.ExtendedStoryLogs)
                if (extendedStoryLog.sceneName == __instance.gameObject.scene.name)
                {
                    if (__instance.storyLogID == extendedStoryLog.storyLogID)
                    {
                        DebugHelper.Log("Updating " + extendedStoryLog.storyLogTitle + "ID", DebugType.Developer);
                        __instance.storyLogID = extendedStoryLog.newStoryLogID;
                    }
                }
        }

        static List<SpawnableMapObject> tempoarySpawnableMapObjectList = new List<SpawnableMapObject>();

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPrefix]
        internal static void RoundManagerSpawnMapObjects_Prefix()
        {
            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);
            foreach (SpawnableMapObject newRandomMapObject in DungeonManager.CurrentExtendedDungeonFlow.SpawnableMapObjects)
            {
                spawnableMapObjects.Add(newRandomMapObject);
                tempoarySpawnableMapObjectList.Add(newRandomMapObject);
            }
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPostfix]
        internal static void RoundManagerSpawnMapObjects_Postfix()
        {
            List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects);
            foreach (SpawnableMapObject spawnableMapObject in tempoarySpawnableMapObjectList)
                spawnableMapObjects.Remove(spawnableMapObject);
            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableMapObjects = spawnableMapObjects.ToArray();
            tempoarySpawnableMapObjectList.Clear();
        }

        internal static GameObject previousHit;
        internal static FootstepSurface previouslyAssignedFootstepSurface;

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PlayerControllerB), "GetCurrentMaterialStandingOn")]
        [HarmonyPrefix]
        internal static bool PlayerControllerBGetCurrentMaterialStandingOn_Prefix(PlayerControllerB __instance)
        {
            /*if (LevelManager.CurrentExtendedLevel.extendedFootstepSurfaces.Count != 0)
                if (Physics.Raycast(new Ray(__instance.thisPlayerBody.position + Vector3.up, -Vector3.up), out RaycastHit hit, 6f, Patches.StartOfRound.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
                    if (hit.collider.gameObject == previousHit)
                        return (false);*/
            return (true);
        }

        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(PlayerControllerB), "GetCurrentMaterialStandingOn")]
        [HarmonyPostfix]
        internal static void PlayerControllerBGetCurrentMaterialStandingOn_Postfix(PlayerControllerB __instance)
        {
            /*if (LevelManager.CurrentExtendedLevel.extendedFootstepSurfaces.Count != 0)
                if (Physics.Raycast(new Ray(__instance.thisPlayerBody.position + Vector3.up, -Vector3.up), out RaycastHit hit, 6f, Patches.StartOfRound.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
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
            */
        }

        //DunGen Optimisation Patches (Credit To LadyRaphtalia, Author Of Scarlet Devil Mansion)
        [HarmonyPriority(harmonyPriority)]
        [HarmonyPatch(typeof(DoorwayPairFinder), "GetDoorwayPairs")]
        [HarmonyPrefix]
        public static bool GetDoorwayPairsPatch(ref DoorwayPairFinder __instance, int? maxCount, ref Queue<DoorwayPair> __result)
        {

            __instance.tileOrder = __instance.CalculateOrderedListOfTiles();
            var doorwayPairs = __instance.PreviousTile == null ?
              __instance.GetPotentialDoorwayPairsForFirstTile() :
              __instance.GetPotentialDoorwayPairsForNonFirstTile();

            var num = doorwayPairs.Count();
            if (maxCount != null)
            {
                num = Mathf.Min(num, maxCount.Value);
            }
            __result = new Queue<DoorwayPair>(num);

            var newList = OrderDoorwayPairs(doorwayPairs, num);
            foreach (var item in newList)
            {
                __result.Enqueue(item);
            }

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

    }
}
