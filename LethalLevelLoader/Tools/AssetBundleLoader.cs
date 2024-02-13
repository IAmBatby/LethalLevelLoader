using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace LethalLevelLoader
{
    public class AssetBundleLoader
    {
        public static AssetBundleLoader Instance;

        public const string specifiedFileExtension = "*.lethalbundle";

        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        internal static List<ExtendedLevel> obtainedExtendedLevelsList = new List<ExtendedLevel>();
        internal static List<ExtendedDungeonFlow> obtainedExtendedDungeonFlowsList = new List<ExtendedDungeonFlow>();

        public enum LoadingStatus { Inactive, Loading, Complete };
        public static LoadingStatus loadingStatus = LoadingStatus.Inactive;

        internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        internal static bool HaveBundlesFinishedLoading
        {
            get
            {
                bool bundlesFinishedLoading = true;
                foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                    if (assetBundle.Value == null)
                        bundlesFinishedLoading = false;
                return (bundlesFinishedLoading);
            }
        }

        internal static int BundlesFinishedLoadingCount
        {
            get
            {
                int bundlesFinishedLoading = 0;
                foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                    if (assetBundle.Value != null)
                        bundlesFinishedLoading++;
                return (bundlesFinishedLoading);
            }
        }

        public delegate void BundlesFinishedLoading();
        public static event BundlesFinishedLoading onBundlesFinishedLoading;

        public delegate void BundleFinishedLoading(AssetBundle assetBundle);
        public static event BundleFinishedLoading onBundleFinishedLoading;

        internal static TextMeshProUGUI loadingBundlesHeaderText;

        internal static bool hasRequestedToLoadMainMenu;

        internal static void RegisterCustomContent(NetworkManager networkManager)
        {
            DebugHelper.Log("Registering Bundle Content!");

            foreach (ExtendedDungeonFlow extendedDungeonFlow in obtainedExtendedDungeonFlowsList)
                RegisterDungeonContent(extendedDungeonFlow, networkManager);
        }

        internal static void LoadBundles(PreInitSceneScript preInitSceneScript)
        {
            DebugHelper.Log("Finding LethalBundles!");

            loadingStatus = LoadingStatus.Loading;

            Instance = new AssetBundleLoader();

            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;

            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                assetBundles.Add(fileInfo.Name, null);
                UpdateLoadingBundlesHeaderText(null);

                preInitSceneScript.StartCoroutine(Instance.LoadBundle(file, fileInfo.Name));
            }
        }

        IEnumerator LoadBundle(string bundleFile, string fileName)
        {
            FileStream fileStream = new FileStream(Path.Combine(Application.streamingAssetsPath, bundleFile), FileMode.Open, FileAccess.Read);
            AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromStreamAsync(fileStream);
            yield return newBundleRequest;

            AssetBundle newBundle = newBundleRequest.assetBundle;

            if (newBundle != null)
            {
                DebugHelper.Log("Loading Custom Content From Bundle: " + newBundle.name);
                assetBundles[fileName] = newBundle;

                if (newBundle.isStreamedSceneAssetBundle == false)
                    foreach (ExtendedLevel extendedLevel in newBundle.LoadAllAssets<ExtendedLevel>())
                    {
                        if (extendedLevel.contentSourceName == string.Empty)
                            extendedLevel.contentSourceName = newBundle.name;
                        obtainedExtendedLevelsList.Add(extendedLevel);
                    }

                onBundleFinishedLoading?.Invoke(newBundle);
            }
            else
            {
                DebugHelper.LogError("Failed To Load Bundle: " + bundleFile);
                assetBundles.Remove(fileName);
                yield break;
            }

            if (HaveBundlesFinishedLoading == true)
            {
                loadingStatus = LoadingStatus.Complete;
                onBundlesFinishedLoading?.Invoke();
            }

            fileStream.Close();
        }

        internal static void LoadContentInBundles()
        {
            bool foundExtendedLevelScene;
            foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(obtainedExtendedLevelsList))
            {
                foundExtendedLevelScene = false;
                foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                    if (assetBundle.Value != null && assetBundle.Value.isStreamedSceneAssetBundle)
                        foreach (string scenePath in assetBundle.Value.GetAllScenePaths())
                            if (GetSceneName(scenePath) == extendedLevel.selectableLevel.sceneName)
                            {
                                //DebugHelper.Log("Found Scene File For ExtendedLevel: " + extendedLevel.selectableLevel.name + ". Scene Path Is: " + scenePath);
                                foundExtendedLevelScene = true;
                                NetworkScenePatcher.AddScenePath(GetSceneName(scenePath));
                            }

                if (foundExtendedLevelScene == false)
                {
                    DebugHelper.LogError("Could Not Find Scene File For ExtendedLevel: " + extendedLevel.selectableLevel.name);
                    obtainedExtendedLevelsList.Remove(extendedLevel);
                }
            }
        }

        internal static void InitializeBundles()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in obtainedExtendedDungeonFlowsList)
            {
                extendedDungeonFlow.Initialize(ContentType.Custom);
                extendedDungeonFlow.manualPlanetNameReferenceList.Add(new StringWithRarity("Tenebrous", 1000));
                DungeonManager.AddExtendedDungeonFlow(extendedDungeonFlow);
            }
            foreach (ExtendedLevel extendedLevel in obtainedExtendedLevelsList)
            {
                //DebugHelper.Log(extendedLevel.contentSourceName);
                if (extendedLevel.selectableLevel != null)
                {
                    //DebugHelper.Log(extendedLevel.selectableLevel.PlanetName);
                    extendedLevel.levelType = ContentType.Custom;
                    extendedLevel.Initialize(extendedLevel.name, generateTerminalAssets: true);
                    PatchedContent.ExtendedLevels.Add(extendedLevel);
                }
                //WarmUpBundleShaders(extendedLevel);
            }
            //DebugHelper.DebugAllLevels();
        }

        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedDungeonFlow() is deprecated. Please move to PatchedContent.RegisterExtendedDungeonFlow() to prevent issues in following updates.");
            PatchedContent.RegisterExtendedDungeonFlow(extendedDungeonFlow);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedLevel() is deprecated. Please move to PatchedContent.RegisterExtendedLevel() to prevent issues in following updates.");
            PatchedContent.RegisterExtendedLevel(extendedLevel);
        }

        internal static void CreateVanillaExtendedLevels(StartOfRound startOfRound)
        {
            DebugHelper.Log("Creating ExtendedLevels For Vanilla SelectableLevels");

            foreach (SelectableLevel selectableLevel in startOfRound.levels)
            {
                ExtendedLevel extendedLevel = ExtendedLevel.Create(selectableLevel, ContentType.Vanilla);

                foreach (CompatibleNoun compatibleRouteNoun in TerminalManager.routeKeyword.compatibleNouns)
                    if (compatibleRouteNoun.noun.name.Contains(ExtendedLevel.GetNumberlessPlanetName(selectableLevel)))
                    {
                        extendedLevel.routeNode = compatibleRouteNoun.result;
                        extendedLevel.routeConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                        extendedLevel.RoutePrice = compatibleRouteNoun.result.itemCost;
                        break;
                    }
                extendedLevel.Initialize("Lethal Company", generateTerminalAssets: false);

                SetVanillaLevelTags(extendedLevel);
                PatchedContent.ExtendedLevels.Add(extendedLevel);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlows()
        {
            //DebugHelper.Log("Creating ExtendedDungeonFlows For Vanilla DungeonFlows");

            if (RoundManager.Instance.dungeonFlowTypes != null)
                foreach (DungeonFlow dungeonFlow in RoundManager.Instance.dungeonFlowTypes)
                    CreateVanillaExtendedDungeonFlow(dungeonFlow);
            else
                DebugHelper.Log("Error! RoundManager dungeonFlowTypes Array Was Null!");
        }

        internal static void CreateVanillaExtendedDungeonFlow(DungeonFlow dungeonFlow)
        {
            AudioClip firstTimeDungeonAudio = null;
            string dungeonDisplayName = string.Empty;

            if (dungeonFlow.name.Contains("Level1"))
            {
                dungeonDisplayName = "Facility";
                firstTimeDungeonAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
            }
            else if (dungeonFlow.name.Contains("Level2"))
            {
                dungeonDisplayName = "Haunted Mansion";
                firstTimeDungeonAudio = RoundManager.Instance.firstTimeDungeonAudios[1];
            }

            ExtendedDungeonFlow extendedDungeonFlow = ExtendedDungeonFlow.Create(dungeonFlow, firstTimeDungeonAudio, "Lethal Company");
            extendedDungeonFlow.dungeonDisplayName = dungeonDisplayName;

            extendedDungeonFlow.Initialize(ContentType.Vanilla);
            DungeonManager.AddExtendedDungeonFlow(extendedDungeonFlow);


            if (extendedDungeonFlow.dungeonID == -1)
                DungeonManager.RefreshDungeonFlowIDs();
            //Gotta assign the right audio later.
        }

        internal static void RegisterDungeonContent(ExtendedDungeonFlow extendedDungeonFlow, NetworkManager networkManager)
        {
            List<string> restoredObjectsDebugList = new List<string>();
            List<string> registeredObjectsDebugList = new List<string>();

            List<GameObject> registeredPrefabs = new List<GameObject>();
            foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
                registeredPrefabs.Add(networkPrefab.Prefab);

            List<SpawnSyncedObject> spawnSyncedObjects = extendedDungeonFlow.dungeonFlow.GetSpawnSyncedObjects();

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
                if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                    spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
                LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);

                if (!registeredObjectsDebugList.Contains(spawnSyncedObject.spawnPrefab.name))
                    registeredObjectsDebugList.Add(spawnSyncedObject.spawnPrefab.name);
            }

            string debugString = "Automatically Restored The Following SpawnablePrefab's In " + extendedDungeonFlow.dungeonDisplayName + ": ";
            foreach (string debug in restoredObjectsDebugList)
                debugString += debug + ", ";
            DebugHelper.Log(debugString);
            debugString = "Automatically Registered The Following SpawnablePrefab's In " + extendedDungeonFlow.dungeonDisplayName + ": ";
            foreach (string debug in registeredObjectsDebugList)
                debugString += debug + ", ";
            DebugHelper.Log(debugString);
        }

        internal static void SetVanillaLevelTags(ExtendedLevel vanillaLevel)
        {
            foreach (IntWithRarity intWithRarity in vanillaLevel.selectableLevel.dungeonFlowTypes)
                if (DungeonManager.TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[intWithRarity.id], out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.manualPlanetNameReferenceList.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, intWithRarity.rarity));

            if (vanillaLevel.NumberlessPlanetName == "Experimentation")
                vanillaLevel.levelTags = new List<string>() { "Wasteland" };
            else if (vanillaLevel.NumberlessPlanetName == "Assurance" || vanillaLevel.NumberlessPlanetName == "Offense")
                vanillaLevel.levelTags = new List<string>() { "Desert", "Canyon" };
            else if (vanillaLevel.NumberlessPlanetName == "Vow" || vanillaLevel.NumberlessPlanetName == "March")
                vanillaLevel.levelTags = new List<string>() { "Forest", "Valley" };
            else if (vanillaLevel.NumberlessPlanetName == "Gordion")
                vanillaLevel.levelTags = new List<string>() { "Company", "Quota" };
            else if (vanillaLevel.NumberlessPlanetName == "Rend" || vanillaLevel.NumberlessPlanetName == "Dine" || vanillaLevel.NumberlessPlanetName == "Titan")
                vanillaLevel.levelTags = new List<string>() { "Snow", "Ice", "Tundra" };

            vanillaLevel.levelTags.Add("Vanilla");
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
            if (loadingStatus != LoadingStatus.Inactive)
                newHeaderText.text = "Loading Bundles: " + assetBundles.First().Key + " (" + BundlesFinishedLoadingCount + " // " + assetBundles.Count + ")";
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
                if (loadingStatus != LoadingStatus.Inactive)
                    loadingBundlesHeaderText.text = "Loading Bundles: " + assetBundles.First().Key + " " + "(" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
                else
                    loadingBundlesHeaderText.text = "Loaded Bundles: " + " (" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
            }
        }


        public static Tile[] GetAllTilesInDungeonFlow(DungeonFlow dungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllTilesInDungeonFlow() is deprecated. Please move to dungeonFlow.GetTiles() to prevent issues in following updates.");
            return (dungeonFlow.GetTiles().ToArray());
        }

        public static RandomMapObject[] GetAllMapObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllMapObjectsInTiles() is deprecated. Please move to dungeonFlow.GetRandomMapObjects() to prevent issues in following updates.");
            return (new List<RandomMapObject>().ToArray());
        }

        public static SpawnSyncedObject[] GetAllSpawnSyncedObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllSpawnSyncedObjectsInTiles() is deprecated. Please move to dungeonFlow.GetSpawnSyncedObjects() to prevent issues in following updates.");
            return (new List<SpawnSyncedObject>().ToArray());
        }
    }
}
