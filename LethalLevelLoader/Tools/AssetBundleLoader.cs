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
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using static LethalLevelLoader.ExtendedContent;
using Action = System.Action;

namespace LethalLevelLoader
{
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

        internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>(); 
        internal static Dictionary<string, string> assetBundleLoadTimes = new Dictionary<string, string>();

        internal static Dictionary<string, List<Action<AssetBundle>>> onLethalBundleLoadedRequestDictionary = new Dictionary<string, List<Action<AssetBundle>>>();
        internal static Dictionary<string, List<Action<ExtendedMod>>> onExtendedModLoadedRequestDictionary = new Dictionary<string, List<Action<ExtendedMod>>>();

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
            //Instance = new AssetBundleLoader();

            onBundlesFinishedLoading += OnBundlesFinishedLoading;

            PatchedContent.VanillaMod = ExtendedMod.Create("LethalCompany", "Zeekerss");
            //PatchedContent.ExtendedMods.Add(PatchedContent.VanillaMod);

            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;

            int counter = 0;
            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!assetBundles.ContainsKey(fileInfo.Name))
                {
                    counter++;
                    assetBundles.Add(fileInfo.Name, null);
                    UpdateLoadingBundlesHeaderText(null);
                    StartCoroutine(Instance.LoadBundle(file, fileInfo.Name));
                }
                else
                    DebugHelper.LogError("Failed To Load Lethalbundle: " + fileInfo.Name + ". A Lethalbundle with an indentical name has already been found.", DebugType.User);
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

        IEnumerator LoadBundle(string bundleFile, string fileName)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //FileStream fileStream = new FileStream(Path.Combine(Application.streamingAssetsPath, bundleFile), FileMode.Open, FileAccess.Read);
            AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleFile));
            yield return newBundleRequest;

            AssetBundle newBundle = newBundleRequest.assetBundle;

            //yield return new WaitUntil(() => newBundle != null);

            if (newBundle != null)
            {
                assetBundles[fileName] = newBundle;

                if (newBundle.isStreamedSceneAssetBundle == false)
                {
                    ExtendedMod[] extendedMods = newBundle.LoadAllAssets<ExtendedMod>();
                    if (extendedMods != null && extendedMods.Length > 0 && extendedMods[0] != null)
                        RegisterExtendedMod(extendedMods[0]);
                    else
                    {
                        DebugHelper.Log("No ExtendedMod Found In Bundle: " + newBundle.name + ". Forcefully Loading ExtendedContent!", DebugType.User);
                        foreach (ExtendedContent extendedContent in newBundle.LoadAllAssets<ExtendedContent>())
                            RegisterNewExtendedContent(extendedContent, newBundle.name);
                    }
                }

                onBundleFinishedLoading?.Invoke(newBundle);
            }
            else
            {
                DebugHelper.LogError("Failed To Load Bundle: " + bundleFile, DebugType.User);
                assetBundles.Remove(fileName);
                yield break;
            }
             
            if (HaveBundlesFinishedLoading == true)
            {
                CurrentLoadingStatus = LoadingStatus.Complete;
                onBundlesFinishedLoading?.Invoke();
            }
            else
            {
                
            }

            //fileStream.Close();
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

        internal static void RegisterExtendedMod(ExtendedMod extendedMod)
        {
            DebugHelper.Log("Found ExtendedMod: " + extendedMod.name, DebugType.User);
            extendedMod.ModNameAliases.Add(extendedMod.ModName);
            ExtendedMod matchingExtendedMod = null;
            foreach (ExtendedMod registeredExtendedMod in obtainedExtendedModsDictionary.Values)
            {
                if (extendedMod.ModMergeSetting == ModMergeSetting.MatchingModName && registeredExtendedMod.ModMergeSetting == ModMergeSetting.MatchingModName)
                {
                    if (registeredExtendedMod.ModName == extendedMod.ModName)
                        matchingExtendedMod = registeredExtendedMod;
                }
                else if (extendedMod.ModMergeSetting == ModMergeSetting.MatchingAuthorName && registeredExtendedMod.ModMergeSetting == ModMergeSetting.MatchingAuthorName)
                {
                    if (registeredExtendedMod.AuthorName == extendedMod.AuthorName)
                        matchingExtendedMod = registeredExtendedMod;
                }
            }

            if (matchingExtendedMod != null)
            {
                if (!matchingExtendedMod.ModName.Contains(matchingExtendedMod.AuthorName))
                {
                    DebugHelper.Log("Renaming ExtendedMod: " + matchingExtendedMod.ModName + " To: " +  matchingExtendedMod.AuthorName + "sMod" + " Due To Upcoming ExtendedMod Merge!", DebugType.Developer);
                    matchingExtendedMod.ModNameAliases.Add(extendedMod.ModName);
                    matchingExtendedMod.ModName = matchingExtendedMod.AuthorName + "sMod";
                    //matchingExtendedMod.name = matchingExtendedMod.ModName;
                }
                DebugHelper.Log("Merging ExtendedMod: " + extendedMod.ModName + " (" + extendedMod.AuthorName + ")" + " With Already Obtained ExtendedMod: " + matchingExtendedMod.ModName + " (" + matchingExtendedMod.AuthorName + ")", DebugType.Developer);
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                {
                    try
                    {
                        matchingExtendedMod.RegisterExtendedContent(extendedContent);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogError(ex, DebugType.User);
                    }
                }
            }
            else
            {
                obtainedExtendedModsDictionary.Add(extendedMod.AuthorName, extendedMod);
                List<ExtendedContent> serializedExtendedContents = new List<ExtendedContent>(extendedMod.ExtendedContents);
                extendedMod.UnregisterAllExtendedContent();
                foreach (ExtendedContent extendedContent in serializedExtendedContents)
                {
                    try
                    {
                        extendedMod.RegisterExtendedContent(extendedContent);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogError(ex, DebugType.User);
                    }
                }
            }
        }

        internal static void RegisterNewExtendedMod()
        {

        }

        public static void AddOnLethalBundleLoadedListener(Action<AssetBundle> invokedFunction, string lethalBundleFileName)
        {
            if (invokedFunction != null && !string.IsNullOrEmpty(lethalBundleFileName))
            {
                if (!onLethalBundleLoadedRequestDictionary.ContainsKey(lethalBundleFileName))
                    onLethalBundleLoadedRequestDictionary.Add(lethalBundleFileName, new List<Action<AssetBundle>>() { invokedFunction });
                else
                    onLethalBundleLoadedRequestDictionary[lethalBundleFileName].Add(invokedFunction);
            }
        }

        public static void AddOnExtendedModLoadedListener(Action<ExtendedMod> invokedFunction, string extendedModAuthorName = null, string extendedModModName = null)
        {
            if (invokedFunction != null && !string.IsNullOrEmpty(extendedModAuthorName))
            {
                if (!onExtendedModLoadedRequestDictionary.ContainsKey(extendedModAuthorName))
                    onExtendedModLoadedRequestDictionary.Add(extendedModAuthorName, new List<Action<ExtendedMod>>() { invokedFunction });
                else
                    onExtendedModLoadedRequestDictionary[extendedModAuthorName].Add(invokedFunction);
            }

            if (invokedFunction != null && !string.IsNullOrEmpty(extendedModModName))
            {
                if (!onExtendedModLoadedRequestDictionary.ContainsKey(extendedModModName))
                    onExtendedModLoadedRequestDictionary.Add(extendedModModName, new List<Action<ExtendedMod>>() { invokedFunction });
                else
                    onExtendedModLoadedRequestDictionary[extendedModModName].Add(invokedFunction);
            }
        }

        internal static void OnBundlesFinishedLoading()
        {
            //foreach (KeyValuePair<string, string> loadedAssetBundles in assetBundleLoadTimes)
                //DebugHelper.Log(loadedAssetBundles.Key + " Loaded In " + loadedAssetBundles.Value, DebugType.User);

            foreach (KeyValuePair<string, ExtendedMod> obtainedExtendedMod in obtainedExtendedModsDictionary)
            {
                PatchedContent.ExtendedMods.Add(obtainedExtendedMod.Value);
                DebugHelper.DebugExtendedMod(obtainedExtendedMod.Value);
            }

            PatchedContent.ExtendedMods = new List<ExtendedMod>(PatchedContent.ExtendedMods.OrderBy(o => o.ModName).ToList());

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                extendedMod.SortRegisteredContent();

            foreach (KeyValuePair<string, List<System.Action<AssetBundle>>> kvp in onLethalBundleLoadedRequestDictionary)
                if (assetBundles.ContainsKey(kvp.Key))
                    foreach (Action<AssetBundle> action in kvp.Value)
                        action(assetBundles[kvp.Key]);

            foreach (KeyValuePair<string, List<Action<ExtendedMod>>> kvp in onExtendedModLoadedRequestDictionary)
                foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                    if (extendedMod.ModNameAliases.Contains(kvp.Key))
                        foreach (Action<ExtendedMod> action in kvp.Value)
                            action(extendedMod);
        }

        //This Function is used to Register new ExtendedConte to LethalLevelLoader, assiging content to it's relevant ExtendedMod or creating a new ExtendedMod if neccasary.
        internal static void RegisterNewExtendedContent(ExtendedContent extendedContent, string fallbackName)
        {
            if (extendedContent == null)
            {
                DebugHelper.LogError("Failed to register new ExtendedContent as it was null!", DebugType.User);
                return;
            }

            ExtendedMod extendedMod = null;
            if (extendedContent is ExtendedLevel extendedLevel)
            {
                if (string.IsNullOrEmpty(extendedLevel.contentSourceName))
                    extendedLevel.contentSourceName = fallbackName;
                extendedMod = GetOrCreateExtendedMod(extendedLevel.contentSourceName);
            }
            else if (extendedContent is ExtendedDungeonFlow extendedDungeonFlow)
            {
                if (string.IsNullOrEmpty(extendedDungeonFlow.contentSourceName))
                    extendedDungeonFlow.contentSourceName = fallbackName;
                extendedMod = GetOrCreateExtendedMod(extendedDungeonFlow.contentSourceName);
            }
            else if (extendedContent is ExtendedItem extendedItem)
            {
                extendedMod = GetOrCreateExtendedMod(extendedItem.Item.itemName.RemoveWhitespace());
            }
            else if (extendedContent is ExtendedEnemyType extendedEnemyType)
            {
                extendedMod = GetOrCreateExtendedMod(extendedEnemyType.EnemyType.enemyName.RemoveWhitespace());
            }
            else if (extendedContent is ExtendedWeatherEffect extendedWeatherEffect)
            {
                //if (extendedWeatherEffect.contentSourceName == string.Empty)
                    //extendedWeatherEffect.contentSourceName = fallbackName;
                //extendedMod = GetOrCreateExtendedMod(extendedWeatherEffect.contentSourceName);
            }
            else if (extendedContent is ExtendedBuyableVehicle extendedBuyableVehicle)
            {
                extendedMod = GetOrCreateExtendedMod(extendedBuyableVehicle.name);
            }

            if (extendedMod != null)
            {
                try
                {
                    extendedMod.RegisterExtendedContent(extendedContent);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError(ex, DebugType.User);
                }
            }
        }

        internal static ExtendedMod GetOrCreateExtendedMod(string contentSourceName)
        {
            if (obtainedExtendedModsDictionary.TryGetValue(contentSourceName, out ExtendedMod extendedMod))
                return (extendedMod);
            else
            {
                DebugHelper.Log("Creating New ExtendedMod: " + contentSourceName, DebugType.Developer);
                ExtendedMod newExtendedMod = ExtendedMod.Create(contentSourceName);
                obtainedExtendedModsDictionary.Add(contentSourceName, newExtendedMod);
                return (newExtendedMod);

            }
        }

        //This function should probably just be in NetworkRegisterContent
        internal static void LoadContentInBundles()
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
                    foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                        if (assetBundle.Value != null && assetBundle.Value.isStreamedSceneAssetBundle)
                            foreach (string scenePath in assetBundle.Value.GetAllScenePaths())
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
            else if (dungeonFlow.name.Contains("Level3"))
            {
                dungeonDisplayName = "Mineshaft";
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
                if (CurrentLoadingStatus != LoadingStatus.Inactive)
                    loadingBundlesHeaderText.text = "Loading Bundles: " + assetBundles.First().Key + " " + "(" + (assetBundles.Count - (assetBundles.Count - BundlesFinishedLoadingCount)) + " // " + assetBundles.Count + ")";
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
