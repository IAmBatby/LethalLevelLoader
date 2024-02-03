using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace LethalLevelLoader
{
    public static class AssetBundleLoader
    {
        public const string specifiedFileExtension = "*.lethalbundle";

        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        internal static List<AssetBundle> loadedAssetBundles = new List<AssetBundle>();
        internal static List<AssetBundle> loadedStreamedAssetBundles = new List<AssetBundle>();

        internal static List<ExtendedLevel> obtainedExtendedLevelsList = new List<ExtendedLevel>();
        internal static List<ExtendedDungeonFlow> obtainedExtendedDungeonFlowsList = new List<ExtendedDungeonFlow>();

        internal static List<string> assetBundle;

        internal static void RegisterCustomContent(NetworkManager networkManager)
        {
            DebugHelper.Log("Registering Bundle Content!");

            foreach (ExtendedDungeonFlow extendedDungeonFlow in obtainedExtendedDungeonFlowsList)
                RegisterDungeonContent(extendedDungeonFlow, networkManager);
        }

        internal static void LoadBundles()
        {
            DebugHelper.Log("Finding LethalBundles!");
            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;

            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
                LoadBundle(file);
        }

        internal static void LoadBundle(string bundleFile)
        {
            FileStream fileStream = new FileStream(Path.Combine(Application.streamingAssetsPath, bundleFile), FileMode.Open, FileAccess.Read);
            AssetBundle newBundle = AssetBundle.LoadFromStream(fileStream);

            if (newBundle != null)
            {
                DebugHelper.Log("Loading Custom Content From Bundle: " + newBundle.name);
                if (newBundle.isStreamedSceneAssetBundle == true)
                    loadedStreamedAssetBundles.Add(newBundle);
                else
                {
                    loadedAssetBundles.Add(newBundle);
                    foreach (ExtendedLevel extendedLevel in newBundle.LoadAllAssets<ExtendedLevel>())
                    {
                        if (extendedLevel.contentSourceName == string.Empty)
                            extendedLevel.contentSourceName = newBundle.name;
                        obtainedExtendedLevelsList.Add(extendedLevel);
                    }
                }
            }
        }

        internal static void LoadContentInBundles()
        {
            bool foundExtendedLevelScene;
            foreach (ExtendedLevel extendedLevel in new List<ExtendedLevel>(obtainedExtendedLevelsList))
            {
                foundExtendedLevelScene = false;
                foreach (AssetBundle streamedAssetBundle in loadedStreamedAssetBundles)
                    foreach (string scenePath in streamedAssetBundle.GetAllScenePaths())
                        if (GetSceneName(scenePath) == extendedLevel.selectableLevel.sceneName)
                        {
                            DebugHelper.Log("Found Scene File For ExtendedLevel: " + extendedLevel.selectableLevel.name + ". Scene Path Is: " + scenePath);
                            foundExtendedLevelScene = true;
                            NetworkScenePatcher.AddScenePath(GetSceneName(scenePath));
                        }

                if (foundExtendedLevelScene == false)
                {
                    DebugHelper.Log("Could Not Find Scene File For ExtendedLevel: " + extendedLevel.selectableLevel.name);
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
                DebugHelper.Log(extendedLevel.contentSourceName);
                if (extendedLevel.selectableLevel != null)
                {
                    DebugHelper.Log(extendedLevel.selectableLevel.PlanetName);
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
            obtainedExtendedDungeonFlowsList.Add(extendedDungeonFlow);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            obtainedExtendedLevelsList.Add(extendedLevel);
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
            DebugHelper.Log("Creating ExtendedDungeonFlows For Vanilla DungeonFlows");

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
    }
}
