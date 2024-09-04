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
using Unity.Services.Relay.RelayAllocations;
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
                FileInfo fileInfo = new FileInfo(file);
                counter++;
                LethalBundleInfo newBundleInfo = new LethalBundleInfo(fileInfo.Name);
                assetBundles.Add(newBundleInfo);
                UpdateLoadingBundlesHeaderText(null);
                StartCoroutine(Instance.LoadBundle(file, newBundleInfo));

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
            AddLoadedListener(onLethalBundleLoadedRequestDictionary, invokedFunction, lethalBundleFileName);
        }

        public static void AddOnExtendedModLoadedListener(Action<ExtendedMod> invokedFunction, string extendedModAuthorName = null, string extendedModModName = null)
        {
            AddLoadedListener(onExtendedModLoadedRequestDictionary, invokedFunction, extendedModModName);
            AddLoadedListener(onExtendedModLoadedRequestDictionary, invokedFunction, extendedModAuthorName);
        }

        internal static void AddLoadedListener<T>(List<(string, List<Action<T>>)> infoList, Action<T> newAction, string comparisonIdentifier)
        {
            if (newAction == null || string.IsNullOrEmpty(comparisonIdentifier)) return;
            foreach ((string, List<Action<T>>) info in infoList)
                if (info.Item1 == comparisonIdentifier)
                {
                    info.Item2.Add(newAction);
                    return;
                }
            infoList.Add((comparisonIdentifier, new List<Action<T>>() { newAction }));
        }

        internal static void OnBundlesFinishedLoading()
        {
            DebugHelper.Log("Bundles Finished Loading, Populating ExtendedMods!", DebugType.User);
            NetworkRegisterLevelContent();

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

            List<SpawnSyncedObject> spawnSyncedObjects = extendedDungeonFlow.DungeonFlow.GetSpawnSyncedObjects();

            for (int i = 0; i < spawnSyncedObjects.Count; i++)
            {
                if (TryRestoreSpawnSyncedObject(networkManager, spawnSyncedObjects[i]) == false)
                {
                    RegisterSpawnSyncedObject(spawnSyncedObjects[i]);
                    if (!registeredObjectsDebugList.Contains(spawnSyncedObjects[i].spawnPrefab.name))
                        registeredObjectsDebugList.Add(spawnSyncedObjects[i].spawnPrefab.name);
                }
                else if (!restoredObjectsDebugList.Contains(spawnSyncedObjects[i].spawnPrefab.name))
                    restoredObjectsDebugList.Add(spawnSyncedObjects[i].spawnPrefab.name);  
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

        internal static bool TryRestoreSpawnSyncedObject(NetworkManager networkManager, SpawnSyncedObject spawnSyncedObject)
        {
            if (spawnSyncedObject == null || spawnSyncedObject.spawnPrefab == null) return (false);

            for (int i = 0; i < networkManager.NetworkConfig.Prefabs.m_Prefabs.Count; i++)
                if (networkManager.NetworkConfig.Prefabs.m_Prefabs[i].Prefab.name ==  spawnSyncedObject.spawnPrefab.name)
                {
                    spawnSyncedObject.spawnPrefab = networkManager.NetworkConfig.Prefabs.m_Prefabs[i].Prefab;
                    return (true);
                }
            return (false);        
        }

        internal static void RegisterSpawnSyncedObject(SpawnSyncedObject spawnSyncedObject)
        {
            if (spawnSyncedObject == null || spawnSyncedObject.spawnPrefab == null) return;
            if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
            LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);
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