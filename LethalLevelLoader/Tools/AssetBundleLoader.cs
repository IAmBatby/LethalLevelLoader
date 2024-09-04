using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using static LethalLevelLoader.ExtendedContent;
using Action = System.Action;

namespace LethalLevelLoader
{
    public class LethalBundleInfo
    {
        public string LethalBundleFileName { get; private set; }
        public AssetBundle LethalAssetBundle { get; private set; }

        public LethalBundleInfo(string newBundleFileName)
        {
            LethalBundleFileName = newBundleFileName;
        }

        public void SetAssetBundle(AssetBundle bundle)
        {
            LethalAssetBundle = bundle;
        }
    }
    public class AssetBundleLoader : MonoBehaviour
    {
        public static AssetBundleLoader Instance;

        internal Plugin pluginInstace;

        public const string specifiedFileExtension = "*.lethalbundle";

        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        internal static Dictionary<string, ExtendedMod> obtainedExtendedModsDictionary = new Dictionary<string, ExtendedMod>();

        public enum LoadingStatus { Inactive, Loading, Complete };
        public static LoadingStatus CurrentLoadingStatus { get; internal set; } = LoadingStatus.Inactive;

        internal static List<LethalBundleInfo> assetBundles = new List<LethalBundleInfo>(); 
        internal static Dictionary<string, string> assetBundleLoadTimes = new Dictionary<string, string>();

        internal static List<(string bundleFileName, List<Action<AssetBundle>> bundleActions)> onLethalBundleLoadedRequestDictionary = new List<(string bundleFileName, List<Action<AssetBundle>> actions)>();
        internal static List<(string bundleFileName, List<Action<ExtendedMod>> modActions)> onExtendedModLoadedRequestDictionary = new List<(string bundleFileName, List<Action<ExtendedMod>> modActions)>();

        internal static bool HaveBundlesFinishedLoading
        {
            get
            {
                bool bundlesFinishedLoading = true;
                foreach (LethalBundleInfo assetBundle in assetBundles)
                    if (assetBundle.LethalAssetBundle == null)
                        bundlesFinishedLoading = false;
                return (bundlesFinishedLoading);
            }
        }

        internal static int BundlesFinishedLoadingCount
        {
            get
            {
                int bundlesFinishedLoading = 0;
                foreach (LethalBundleInfo assetBundle in assetBundles)
                    if (assetBundle.LethalAssetBundle != null)
                        bundlesFinishedLoading++;
                return (bundlesFinishedLoading);
            }
        }

        public delegate void BundlesFinishedLoading();
        public static event BundlesFinishedLoading onBundlesFinishedLoading;

        public delegate void BundleFinishedLoading(AssetBundle assetBundle);
        public static event BundleFinishedLoading onBundleFinishedLoading;

        internal static TextMeshProUGUI loadingBundlesHeaderText;

        internal static bool noBundlesFound = false;

        internal static bool hasRequestedToLoadMainMenu;

        //This Function is used to Register NetworkPrefabs to the GameNetworkManager on GameNetworkManager.Start()
        internal static void NetworkRegisterCustomContent(NetworkManager networkManager)
        {
            DebugHelper.Log("Registering Bundle Content!", DebugType.User);

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
            {
                foreach (ExtendedDungeonFlow extendedDungeonFlow in extendedMod.ExtendedDungeonFlows)
                    NetworkRegisterDungeonContent(extendedDungeonFlow, networkManager);

                foreach (ExtendedItem extendedItem in extendedMod.ExtendedItems)
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(extendedItem.Item.spawnPrefab);

                foreach (ExtendedEnemyType extendedEnemyType in extendedMod.ExtendedEnemyTypes)
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(extendedEnemyType.EnemyType.enemyPrefab);

                foreach (ExtendedBuyableVehicle extendedBuyableVehicle in extendedMod.ExtendedBuyableVehicles)
                {
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(extendedBuyableVehicle.BuyableVehicle.vehiclePrefab);
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(extendedBuyableVehicle.BuyableVehicle.secondaryPrefab);
                }
            }
        }

        internal void LoadBundles()
        {
            DebugHelper.Log("Finding LethalBundles!", DebugType.User);

            CurrentLoadingStatus = LoadingStatus.Loading;
            Instance = this;

            onBundlesFinishedLoading += OnBundlesFinishedLoading;

            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;

            int counter = 0;
            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
            {
                counter++;
                FileInfo fileInfo = new FileInfo(file);
<<<<<<< Updated upstream
                assetBundles.Add(fileInfo.Name, null);
                UpdateLoadingBundlesHeaderText(null);

                //preInitSceneScript.StartCoroutine(Instance.LoadBundle(file, fileInfo.Name));
                this.StartCoroutine(Instance.LoadBundle(file, fileInfo.Name));
=======
                counter++;
                LethalBundleInfo newBundleInfo = new LethalBundleInfo(fileInfo.Name);
                assetBundles.Add(newBundleInfo);
                UpdateLoadingBundlesHeaderText(null);
                StartCoroutine(Instance.LoadBundle(file, newBundleInfo));
>>>>>>> Stashed changes
            }
            if (counter == 0)
            {
                DebugHelper.Log("No Bundles Found!", DebugType.User);
                noBundlesFound = true;
                CurrentLoadingStatus = LoadingStatus.Complete;
                onBundlesFinishedLoading?.Invoke();
            }
        }

        internal static void OnBundlesFinishedLoadingInvoke()
        {
            onBundlesFinishedLoading?.Invoke();
        }

        IEnumerator LoadBundle(string bundleFile, LethalBundleInfo bundleInfo)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleFile));
            yield return newBundleRequest;

            AssetBundle newBundle = newBundleRequest.assetBundle;

            if (newBundle != null)
            {
                bundleInfo.SetAssetBundle(newBundle);


                if (newBundle.isStreamedSceneAssetBundle == false)
                {
                    ExtendedMod[] extendedMods = newBundle.LoadAllAssets<ExtendedMod>();
                    if (extendedMods.Length > 0)
                        foreach (ExtendedMod extendedMod in extendedMods)
                            ContentManager.RegisterExtendedMod(extendedMod);
                    else
                    {
                        DebugHelper.Log("No ExtendedMod Found In Bundle: " + newBundle.name + ". Forcefully Loading ExtendedContent!", DebugType.User);
                        ContentManager.RegisterExtendedMod(ExtendedMod.Create(newBundle.name, extendedContents: newBundle.LoadAllAssets<ExtendedContent>()));
                    }
                }

                onBundleFinishedLoading?.Invoke(newBundle);
            }
            else
            {
                DebugHelper.LogError("Failed To Load Bundle: " + bundleFile, DebugType.User);
                assetBundles.Remove(bundleInfo);
                yield break;
            }
             
            if (HaveBundlesFinishedLoading == true)
            {
                CurrentLoadingStatus = LoadingStatus.Complete;
                onBundlesFinishedLoading?.Invoke();
            }

            stopWatch.Stop();
            try
            {
                assetBundleLoadTimes.Add(bundleFile.Substring(bundleFile.LastIndexOf("\\") + 1), $"{stopWatch.Elapsed.TotalSeconds:0.##} Seconds. ({stopWatch.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                DebugHelper.LogError(ex, DebugType.User);
            }
        }

        public static void AddOnLethalBundleLoadedListener(Action<AssetBundle> invokedFunction, string lethalBundleFileName)
        {
            if (invokedFunction != null && !string.IsNullOrEmpty(lethalBundleFileName))
            {
                foreach ((string, List<Action<AssetBundle>>) info in onLethalBundleLoadedRequestDictionary)
                {
                    if (info.Item1 == lethalBundleFileName)
                    {
                        info.Item2.Add(invokedFunction);
                        return;
                    }
                }
                onLethalBundleLoadedRequestDictionary.Add((lethalBundleFileName, new List<Action<AssetBundle>>() { invokedFunction }));
            }
        }

        public static void AddOnExtendedModLoadedListener(Action<ExtendedMod> invokedFunction, string extendedModAuthorName = null, string extendedModModName = null)
        {
            bool foundResult = false;
            if (invokedFunction != null && !string.IsNullOrEmpty(extendedModAuthorName))
            {
                foreach ((string, List<Action<ExtendedMod>>) info in onExtendedModLoadedRequestDictionary)
                {
                    if (info.Item1 == extendedModAuthorName)
                    {
                        info.Item2.Add(invokedFunction);
                        foundResult = true;
                        break;
                    }
                }
                if (foundResult == false)
                    onExtendedModLoadedRequestDictionary.Add((extendedModAuthorName, new List<Action<ExtendedMod>>() { invokedFunction }));
            }

            if (invokedFunction != null && !string.IsNullOrEmpty(extendedModModName))
            {
                foreach ((string, List<Action<ExtendedMod>>) info in onExtendedModLoadedRequestDictionary)
                {
                    if (info.Item1 == extendedModModName)
                    {
                        info.Item2.Add(invokedFunction);
                        return;
                    }
                }
                onExtendedModLoadedRequestDictionary.Add((extendedModModName, new List<Action<ExtendedMod>>() { invokedFunction }));
            }
        }

        internal static void OnBundlesFinishedLoading()
        {
            foreach (KeyValuePair<string, ExtendedMod> obtainedExtendedMod in obtainedExtendedModsDictionary)
            {
                PatchedContent.ExtendedMods.Add(obtainedExtendedMod.Value);
                DebugHelper.DebugExtendedMod(obtainedExtendedMod.Value);
            }

            PatchedContent.ExtendedMods = new List<ExtendedMod>(PatchedContent.ExtendedMods.OrderBy(o => o.ModName).ToList());

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                extendedMod.SortRegisteredContent();

            foreach ((string, List<Action<AssetBundle>>) kvp in onLethalBundleLoadedRequestDictionary)
                foreach (LethalBundleInfo bundleInfo in assetBundles)
                if (bundleInfo.LethalBundleFileName == kvp.Item1)
                    foreach (Action<AssetBundle> action in kvp.Item2)
                        action(bundleInfo.LethalAssetBundle);

            foreach ((string, List<Action<ExtendedMod>>) kvp in onExtendedModLoadedRequestDictionary)
                foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                    if (extendedMod.ModNameAliases.Contains(kvp.Item1))
                        foreach (Action<ExtendedMod> action in kvp.Item2)
                            action(extendedMod);
        }

        internal static void NetworkRegisterLevelContent()
        {
            bool foundExtendedLevelScene;
            List<ExtendedMod> obtainedExtendedModsList = obtainedExtendedModsDictionary.Values.OrderBy(o => o.ModName).ToList();
            List<string> sceneNames = new List<string>();

            foreach (ExtendedMod extendedMod in obtainedExtendedModsList)
                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(extendedMod.ExtendedLevels))
                {
                    if (!sceneNames.Contains(extendedLevel.SelectableLevel.sceneName))
                        sceneNames.Add(extendedLevel.SelectableLevel.sceneName);
                    foreach (StringWithRarity sceneName in extendedLevel.SceneSelections)
                        if (!sceneNames.Contains(sceneName.Name))
                            sceneNames.Add(sceneName.Name);
                }

            foreach (ExtendedMod extendedMod in obtainedExtendedModsList)
            {
                foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(extendedMod.ExtendedLevels))
                {
                    foundExtendedLevelScene = false;
                    string debugString = "Could Not Find Scene File For ExtendedLevel: " + extendedLevel.SelectableLevel.name + ", Unregistering Early. \nSelectable Scene Name Is: " + extendedLevel.SelectableLevel.sceneName + ". Scenes Found In Bundles Are: " + "\n";
                    foreach (LethalBundleInfo assetBundle in assetBundles)
                        if (assetBundle.LethalAssetBundle != null && assetBundle.LethalAssetBundle.isStreamedSceneAssetBundle)
                            foreach (string scenePath in assetBundle.LethalAssetBundle.GetAllScenePaths())
                            {
                                debugString += ", " + GetSceneName(scenePath);
                                if (sceneNames.Contains(GetSceneName(scenePath)))
                                {
                                    //DebugHelper.Log("Found Scene File For ExtendedLevel: " + extendedLevel.selectableLevel.name + ". Scene Path Is: " + scenePath);
                                    foundExtendedLevelScene = true;
                                    NetworkScenePatcher.AddScenePath(GetSceneName(scenePath));
                                    if (!PatchedContent.AllLevelSceneNames.Contains(GetSceneName(scenePath)))
                                        PatchedContent.AllLevelSceneNames.Add(GetSceneName(scenePath));
                                }
                            }

                    if (foundExtendedLevelScene == false)
                    {
                        DebugHelper.LogError(debugString, DebugType.User);
                        extendedMod.UnregisterExtendedContent(extendedLevel);
                    }
                }
            }

            foreach (string loadedSceneName in PatchedContent.AllLevelSceneNames)
                DebugHelper.Log("Loaded SceneName: " + loadedSceneName, DebugType.Developer);
        }

<<<<<<< Updated upstream
        internal static void InitializeBundles()
        {
            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
            {
                foreach (ExtendedLevel extendedLevel in extendedMod.ExtendedLevels)
                {
                    extendedLevel.ContentType = ContentType.Custom;
                    extendedLevel.Initialize(extendedLevel.name, generateTerminalAssets: true);
                    PatchedContent.ExtendedLevels.Add(extendedLevel);
                }
                foreach (ExtendedDungeonFlow extendedDungeonFlow in extendedMod.ExtendedDungeonFlows)
                {
                    extendedDungeonFlow.ContentType = ContentType.Custom;
                    extendedDungeonFlow.Initialize();
                    //extendedDungeonFlow.manualPlanetNameReferenceList.Add(new StringWithRarity("Tenebrous", 1000));
                    PatchedContent.ExtendedDungeonFlows.Add(extendedDungeonFlow); 
                }
                foreach (ExtendedItem extendedItem in extendedMod.ExtendedItems)
                {
                    extendedItem.ContentType = ContentType.Custom;
                    extendedItem.Initialize();
                    PatchedContent.ExtendedItems.Add(extendedItem);
                }
                foreach (ExtendedEnemyType extendedEnemyType in extendedMod.ExtendedEnemyTypes)
                {
                    extendedEnemyType.ContentType = ContentType.Custom;
                    extendedEnemyType.Initalize();
                    PatchedContent.ExtendedEnemyTypes.Add(extendedEnemyType);
                }
                foreach (ExtendedWeatherEffect extendedWeatherEffect in extendedMod.ExtendedWeatherEffects)
                    PatchedContent.ExtendedWeatherEffects.Add(extendedWeatherEffect);
                foreach (ExtendedBuyableVehicle extendedBuyableVehicle in extendedMod.ExtendedBuyableVehicles)
                {
                    extendedBuyableVehicle.ContentType = ContentType.Custom;
                    PatchedContent.ExtendedBuyableVehicles.Add(extendedBuyableVehicle);
                }    
            }
            //DebugHelper.DebugAllLevels();
        }

        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedDungeonFlow() is deprecated. Please move to PatchedContent.RegisterExtendedDungeonFlow() to prevent issues in following updates.", DebugType.Developer);
            PatchedContent.RegisterExtendedDungeonFlow(extendedDungeonFlow);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedLevel() is deprecated. Please move to PatchedContent.RegisterExtendedLevel() to prevent issues in following updates.", DebugType.Developer);
            PatchedContent.RegisterExtendedLevel(extendedLevel);
        }

        internal static void CreateVanillaExtendedLevels(StartOfRound startOfRound)
        {
            DebugHelper.Log("Creating ExtendedLevels For Vanilla SelectableLevels", DebugType.Developer);

            foreach (SelectableLevel selectableLevel in startOfRound.levels)
            {
                ExtendedLevel extendedLevel = ExtendedLevel.Create(selectableLevel);

                foreach (CompatibleNoun compatibleRouteNoun in TerminalManager.routeKeyword.compatibleNouns)
                    if (compatibleRouteNoun.noun.name.Contains(ExtendedLevel.GetNumberlessPlanetName(selectableLevel)))
                    {
                        extendedLevel.RouteNode = compatibleRouteNoun.result;
                        extendedLevel.RouteConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                        extendedLevel.RoutePrice = compatibleRouteNoun.result.itemCost;
                        break;
                    }
                PatchedContent.AllLevelSceneNames.Add(extendedLevel.SelectableLevel.sceneName);

                extendedLevel.Initialize("Lethal Company", generateTerminalAssets: false);
                extendedLevel.name = extendedLevel.NumberlessPlanetName + "ExtendedLevel";

                PatchedContent.ExtendedLevels.Add(extendedLevel);
                PatchedContent.VanillaMod.RegisterExtendedContent(extendedLevel);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlows()
        {
            //DebugHelper.Log("Creating ExtendedDungeonFlows For Vanilla DungeonFlows");

            if (Patches.RoundManager.dungeonFlowTypes != null)
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    CreateVanillaExtendedDungeonFlow(indoorMapType.dungeonFlow);
            else
                DebugHelper.Log("Error! RoundManager dungeonFlowTypes Array Was Null!", DebugType.User);
        }

        internal static void CreateVanillaExtendedItems()
        {
            foreach (Item scrapItem in OriginalContent.Items)
            {
                ExtendedItem extendedVanillaItem = ExtendedItem.Create(scrapItem, PatchedContent.VanillaMod, ContentType.Vanilla);
                extendedVanillaItem.IsBuyableItem = false;
                PatchedContent.ExtendedItems.Add(extendedVanillaItem);
            }


            Terminal terminal = TerminalManager.Terminal;
            int counter = 0;
            foreach (Item item in terminal.buyableItemsList)
            {
                ExtendedItem extendedVanillaItem = ExtendedItem.Create(item, PatchedContent.VanillaMod, ContentType.Vanilla);
                extendedVanillaItem.IsBuyableItem = true;

                foreach (CompatibleNoun compatibleNoun in TerminalManager.buyKeyword.compatibleNouns)
                    if (compatibleNoun.result.buyItemIndex == counter)
                    {
                        extendedVanillaItem.BuyNode = compatibleNoun.result;
                        extendedVanillaItem.BuyConfirmNode = compatibleNoun.result.terminalOptions[0].result;
                        foreach (CompatibleNoun infoCompatibleNoun in TerminalManager.routeInfoKeyword.compatibleNouns)
                            if (infoCompatibleNoun.noun.word == compatibleNoun.noun.word)
                                extendedVanillaItem.BuyInfoNode = infoCompatibleNoun.result;
                    }
                PatchedContent.ExtendedItems.Add(extendedVanillaItem);
                counter++;
            }
        }

        internal static void CreateVanillaExtendedEnemyTypes()
        {
            foreach (EnemyType enemyType in OriginalContent.Enemies)
            {
                ExtendedEnemyType newExtendedEnemyType = ExtendedEnemyType.Create(enemyType, PatchedContent.VanillaMod, ContentType.Vanilla);
                PatchedContent.ExtendedEnemyTypes.Add(newExtendedEnemyType);
                ScanNodeProperties enemyScanNode = newExtendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                if (enemyScanNode != null)
                {
                    newExtendedEnemyType.ScanNodeProperties = enemyScanNode;
                    newExtendedEnemyType.EnemyID = enemyScanNode.creatureScanID;
                    newExtendedEnemyType.EnemyInfoNode = Patches.Terminal.enemyFiles[newExtendedEnemyType.EnemyID];
                    if (newExtendedEnemyType.EnemyInfoNode != null)
                        newExtendedEnemyType.InfoNodeVideoClip = newExtendedEnemyType.EnemyInfoNode.displayVideo;
                    newExtendedEnemyType.EnemyDisplayName = enemyScanNode.headerText;
                }
                else
                    newExtendedEnemyType.EnemyDisplayName = enemyType.enemyName;
            }
        }

        internal static void CreateVanillaExtendedWeatherEffects(StartOfRound startOfRound, TimeOfDay timeOfDay)
        {
            foreach (LevelWeatherType levelWeatherType in Enum.GetValues(typeof(LevelWeatherType)))
            {
                ExtendedWeatherEffect newExtendedWeatherEffect;
                if (levelWeatherType != LevelWeatherType.None)
                    newExtendedWeatherEffect = ExtendedWeatherEffect.Create(levelWeatherType, timeOfDay.effects[(int)levelWeatherType], levelWeatherType.ToString(), ContentType.Vanilla);
                else
                    newExtendedWeatherEffect = ExtendedWeatherEffect.Create(levelWeatherType, null, null, levelWeatherType.ToString(), ContentType.Vanilla);
                
                PatchedContent.ExtendedWeatherEffects.Add(newExtendedWeatherEffect);
                PatchedContent.VanillaMod.ExtendedWeatherEffects.Add(newExtendedWeatherEffect);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlow(DungeonFlow dungeonFlow)
        {
            AudioClip firstTimeDungeonAudio = null;
            string dungeonDisplayName = string.Empty;

            if (dungeonFlow.name.Contains("Level1"))
            {
                dungeonDisplayName = "Facility";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[0];
            }
            else if (dungeonFlow.name.Contains("Level2"))
            {
                dungeonDisplayName = "Haunted Mansion";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[1];
            }

            ExtendedDungeonFlow extendedDungeonFlow = ExtendedDungeonFlow.Create(dungeonFlow, firstTimeDungeonAudio);
            extendedDungeonFlow.DungeonName = dungeonDisplayName;

            extendedDungeonFlow.Initialize();
            PatchedContent.VanillaMod.RegisterExtendedContent(extendedDungeonFlow);
            PatchedContent.ExtendedDungeonFlows.Add(extendedDungeonFlow);

            if (extendedDungeonFlow.DungeonID == -1)
                DungeonManager.RefreshDungeonFlowIDs();
            //Gotta assign the right audio later.
        }

        internal static void CreateVanillaExtendedBuyableVehicles()
        {
            foreach (BuyableVehicle vanillaBuyableVehicle in Patches.Terminal.buyableVehicles)
                CreateVanillaExtendedBuyableVehicle(vanillaBuyableVehicle);
        }

        internal static void CreateVanillaExtendedBuyableVehicle(BuyableVehicle buyableVehicle)
        {
            ExtendedBuyableVehicle newExtendedVanillaBuyableVehicle = ExtendedBuyableVehicle.Create(buyableVehicle);
            PatchedContent.VanillaMod.RegisterExtendedContent(newExtendedVanillaBuyableVehicle);
            PatchedContent.ExtendedBuyableVehicles.Add(newExtendedVanillaBuyableVehicle);
        }

=======
>>>>>>> Stashed changes
        internal static void NetworkRegisterDungeonContent(ExtendedDungeonFlow extendedDungeonFlow, NetworkManager networkManager)
        {
            if (extendedDungeonFlow == null)
            {
                DebugHelper.LogError("Cannot Network Register Null ExtendedDungeonFlow!", DebugType.User);
                return;
            }
            if (extendedDungeonFlow.DungeonFlow == null)
            {
                DebugHelper.LogError("Cannot Network Register ExtendedDungeonFlow: " + extendedDungeonFlow.name + " Due To Null DungeonFlow!", DebugType.User);
                return;
            }
            List<string> restoredObjectsDebugList = new List<string>();
            List<string> registeredObjectsDebugList = new List<string>();

            List<GameObject> registeredPrefabs = new List<GameObject>();
            foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
                registeredPrefabs.Add(networkPrefab.Prefab);

            List<SpawnSyncedObject> spawnSyncedObjects = extendedDungeonFlow.DungeonFlow.GetSpawnSyncedObjects();

            foreach (GameObject registeredPrefab in registeredPrefabs)
            {
                foreach (SpawnSyncedObject spawnSyncedObject in new List<SpawnSyncedObject>(spawnSyncedObjects))
                    if (spawnSyncedObject.spawnPrefab != null && spawnSyncedObject.spawnPrefab.name == registeredPrefab.name)
                    {
                        spawnSyncedObject.spawnPrefab = registeredPrefab;
                        spawnSyncedObjects.Remove(spawnSyncedObject);
                        if (!restoredObjectsDebugList.Contains(registeredPrefab.name))
                            restoredObjectsDebugList.Add(registeredPrefab.name);
                    }

            }
            foreach (SpawnSyncedObject spawnSyncedObject in spawnSyncedObjects)
            {
                if (spawnSyncedObject != null && spawnSyncedObject.spawnPrefab != null)
                {
                    if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                        spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
                    LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);

                    if (!registeredObjectsDebugList.Contains(spawnSyncedObject.spawnPrefab.name))
                        registeredObjectsDebugList.Add(spawnSyncedObject.spawnPrefab.name);
                }
            }

            string debugString = "Automatically Restored The Following SpawnablePrefab's In " + extendedDungeonFlow.DungeonFlow.name + ": ";
            foreach (string debug in restoredObjectsDebugList)
                debugString += debug + ", ";
            DebugHelper.Log(debugString, DebugType.Developer);
            debugString = "Automatically Registered The Following SpawnablePrefab's In " + extendedDungeonFlow.DungeonFlow.name + ": ";
            foreach (string debug in registeredObjectsDebugList)
                debugString += debug + ", ";
            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static void SetVanillaLevelTags(ExtendedLevel vanillaLevel)
        {
            foreach (IntWithRarity intWithRarity in vanillaLevel.SelectableLevel.dungeonFlowTypes)
                if (DungeonManager.TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonFlowTypes[intWithRarity.id].dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, intWithRarity.rarity));

            if (vanillaLevel.SelectableLevel.sceneName == "Level4March")
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    if (indoorMapType.dungeonFlow.name == "Level1Flow3Exits")
                        if (DungeonManager.TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out ExtendedDungeonFlow marchDungeonFlow))
                            marchDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, 300));

            foreach (CompatibleNoun infoNoun in TerminalManager.routeInfoKeyword.compatibleNouns)
                if (infoNoun.noun.word == vanillaLevel.NumberlessPlanetName.ToLower())
                {
                    vanillaLevel.InfoNode = infoNoun.result;
                    break;
                }
        }

        internal static string GetSceneName(string scenePath)
        {
            return (scenePath.Substring(scenePath.LastIndexOf('/') + 1).Replace(".unity", ""));
        }

        internal static void CreateLoadingBundlesHeaderText(PreInitSceneScript preInitSceneScript)
        {
            GameObject newHeader = GameObject.Instantiate(preInitSceneScript.headerText.gameObject, preInitSceneScript.headerText.transform.parent);
            RectTransform newHeaderRectTransform = newHeader.GetComponent<RectTransform>();
            TextMeshProUGUI newHeaderText = newHeader.GetComponent<TextMeshProUGUI>();

            newHeaderRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newHeaderRectTransform.anchorMax = new Vector2(0.0f, 0.0f);
            newHeaderRectTransform.offsetMin = new Vector2(0, -150);
            newHeaderRectTransform.offsetMax = new Vector2(0, -150);
            newHeaderRectTransform.anchoredPosition = new Vector2(0, -150);
            if (CurrentLoadingStatus != LoadingStatus.Inactive)
                newHeaderText.text = "Loading Bundles: " + assetBundles.First().LethalBundleFileName + " (" + BundlesFinishedLoadingCount + " // " + assetBundles.Count + ")";
            else
                newHeaderText.text = "Loading Bundles: " + " (" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
            newHeaderText.color = new Color(0.641f, 0.641f, 0.641f, 1);
            newHeaderText.fontSize = 20;
            //newHeaderRectTransform.sizeDelta = new Vector2(400, 47);
            newHeaderText.overflowMode = TextOverflowModes.Overflow;
            newHeaderText.enableWordWrapping = false;
            newHeaderText.alignment = TextAlignmentOptions.Center;

            loadingBundlesHeaderText = newHeaderText;

            onBundleFinishedLoading += UpdateLoadingBundlesHeaderText;


        }

        internal static void UpdateLoadingBundlesHeaderText(AssetBundle _)
        {
            if (loadingBundlesHeaderText != null)
            {
                if (CurrentLoadingStatus != LoadingStatus.Inactive)
                    loadingBundlesHeaderText.text = "Loading Bundles: " + assetBundles.First().LethalBundleFileName + " " + "(" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
                else
                    loadingBundlesHeaderText.text = "Loaded Bundles: " + " (" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
            }
        }


        public static Tile[] GetAllTilesInDungeonFlow(DungeonFlow dungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllTilesInDungeonFlow() is deprecated. Please move to dungeonFlow.GetTiles() to prevent issues in following updates.", DebugType.Developer);
            return (dungeonFlow.GetTiles().ToArray());
        }

        public static RandomMapObject[] GetAllMapObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllMapObjectsInTiles() is deprecated. Please move to dungeonFlow.GetRandomMapObjects() to prevent issues in following updates.", DebugType.Developer);
            return (new List<RandomMapObject>().ToArray());
        }

        public static SpawnSyncedObject[] GetAllSpawnSyncedObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllSpawnSyncedObjectsInTiles() is deprecated. Please move to dungeonFlow.GetSpawnSyncedObjects() to prevent issues in following updates.", DebugType.Developer);
            return (new List<SpawnSyncedObject>().ToArray());
        }
    }
}
