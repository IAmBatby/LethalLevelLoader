using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalLevelLoader.AssetBundles
{
    internal class AssetBundleLoader : MonoBehaviour
    {
        private static AssetBundleLoader instance;
        public static AssetBundleLoader Instance
        {
            get
            {
                if (instance == null)
                    instance = Object.FindFirstObjectByType<AssetBundleLoader>();
                return (instance);
            }
        }

        internal static DirectoryInfo pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;

        private Dictionary<string, AssetBundleInfo> assetBundleInfoDirectoryDict = new Dictionary<string, AssetBundleInfo>();

        internal List<AssetBundleInfo> AssetBundleInfos = new List<AssetBundleInfo>();
        internal List<AssetBundleGroup> AssetBundleGroups { get; private set; } = new List<AssetBundleGroup>();

        public static ExtendedEvent OnBeforeProcessBundles = new ExtendedEvent();
        public static ExtendedEvent OnBundlesFinishedProcessing = new ExtendedEvent();

        public static ExtendedEvent<AssetBundleInfo> OnBundleLoaded = new ExtendedEvent<AssetBundleInfo>();
        public static ExtendedEvent<AssetBundleInfo> OnBundleUnloaded = new ExtendedEvent<AssetBundleInfo>();

        private static Dictionary<(string, string), List<ParameterEvent<AssetBundleGroup>>> processedCallbacksDict = new Dictionary<(string, string), List<ParameterEvent<AssetBundleGroup>>>();

        //Semi legacy
        internal static Dictionary<string, List<Action<AssetBundle>>> onLethalBundleLoadedRequestDict = new Dictionary<string, List<Action<AssetBundle>>>();


        private static int processedBundleCount;
        private static int requestedBundleCount;

        private void OnEnable()
        {
             instance = this;
            OnBundlesFinishedProcessing.AddListener(LethalLevelLoader.AssetBundleLoader.InvokeBundlesFinishedLoading);
        }

        internal static bool AllowLoading { get; set; } = true;

        private bool onCooldown;

        public static bool LoadAllBundlesRequest(DirectoryInfo directory = null, string specifiedFileName = null, string specifiedFileExtension = null, ParameterEvent<AssetBundleGroup> onProcessedCallback = null)
        {
            if (AllowLoading == false)
            {
                DebugHelper.LogError("Cannot Process LoadAllBundles() Request As We Cannot Load Bundles At This Time.", DebugType.User);
                return (false);
            }

            //TODO: Should cache this
            int foundFilesCount = 0;
            if (directory == null) directory = pluginsFolder;
            if (specifiedFileExtension == null) specifiedFileExtension = ".*";
            if (specifiedFileName == null) specifiedFileName = "*";
            foreach (string filePath in Directory.GetFiles(directory.FullName, specifiedFileName + specifiedFileExtension, SearchOption.AllDirectories))
                foundFilesCount++;

            if (foundFilesCount == 0)
            {
                DebugHelper.Log("No Files Found, Cancelling LoadAllBundlesRequest!", DebugType.User);
                return (false);
            }

            LoadAllBundles(directory, specifiedFileName, specifiedFileExtension, onProcessedCallback);
            return (true);
        }

        private IEnumerator ClearCacheRoutine()
        {
            AsyncOperation unloadUnused = Resources.UnloadUnusedAssets();
            yield return unloadUnused;
            Caching.ClearCache();
            GC.Collect();
            unloadUnused = Resources.UnloadUnusedAssets();
            yield return unloadUnused;
        }

        internal static void ClearCache()
        {
            Instance.StartCoroutine(Instance.ClearCacheRoutine());
        }

        private static void LoadAllBundles(DirectoryInfo directory = null, string specifiedFileName = null, string specifiedFileExtension = null, ParameterEvent<AssetBundleGroup> onProcessedCallback = null)
        {

            AllowLoading = false;
            processedBundleCount = 0;
            requestedBundleCount = 0;

            if (directory == null) directory = pluginsFolder;
            if (specifiedFileExtension == null) specifiedFileExtension = ".*";
            if (specifiedFileName == null) specifiedFileName = "*";

            string callbackName = specifiedFileName == "*" ? string.Empty : specifiedFileName;
            string callbackExtension = specifiedFileExtension == ".*" ? string.Empty : specifiedFileExtension;
            string callBack = callbackName.ToLowerInvariant() + callbackExtension.ToLowerInvariant();
            if (onProcessedCallback != null)
            {
                if (processedCallbacksDict.TryGetValue((directory.FullName, callBack), out List<ParameterEvent<AssetBundleGroup>> list))
                    list.Add(onProcessedCallback);
                else
                    processedCallbacksDict.Add((directory.FullName, callBack), new List<ParameterEvent<AssetBundleGroup>>() { onProcessedCallback });
            }

            foreach (string filePath in Directory.GetFiles(directory.FullName, specifiedFileName + specifiedFileExtension, SearchOption.AllDirectories))
            {
                requestedBundleCount++;
                AssetBundleInfo newInfo = new AssetBundleInfo(Instance, filePath);
                newInfo.OnBundleLoaded.AddListener(OnAssetBundleLoadChanged);
                Instance.AssetBundleInfos.Add(newInfo);
            }

            if (requestedBundleCount > 0)
            {
                OnBeforeProcessBundles.Invoke();
                OnBundleLoaded.AddListener(ProcessInitialBundleLoading);
                foreach (AssetBundleInfo info in Instance.AssetBundleInfos)
                    info.TryLoadBundle();
            }
            else
            {
                DebugHelper.Log("No Bundles Found!", DebugType.User);
                AllowLoading = true;
                OnBundlesFinishedProcessing.Invoke();
            }

        }

        private static void OnAssetBundleLoadChanged(AssetBundleInfo info)
        {
            if (info.IsAssetBundleLoaded)
                OnBundleLoaded.Invoke(info);
            else
            {
                OnBundleUnloaded.Invoke(info);
                if (Plugin.IsSetupComplete)
                    ClearCache();
            }
        }

        private static void ProcessInitialBundleLoading(AssetBundleInfo info)
        {
            DebugHelper.Log("Processing Bundle: " + info.AssetBundleName, DebugType.IAmBatby);

            //Semi arbitrary
            if (info.AssetBundleMode == AssetBundleType.Streaming)
                info.IsHotReloadable = true;
            //info.TryUnloadBundle();

            processedBundleCount++;
            if (processedBundleCount == instance.AssetBundleInfos.Count)
            {
                DebugHelper.Log("Finished Processing Bundles!", DebugType.User);
                processedBundleCount = 0;
                OnBundleLoaded.RemoveListener(ProcessInitialBundleLoading);
                OnInitialBundlesProcessed();
            }
        }

        private static void OnInitialBundlesProcessed()
        {
            Instance.AssetBundleGroups.Clear();
            List<UniqueSceneGroup> uniqueSceneGroups = new List<UniqueSceneGroup>();


            //These are scene names and not paths so we need to ensure to correctly handle any duplicate scene names
            //Pretty sure LC and even Unity would reject duplicate scene names anyway but it bothers me
            Dictionary<string, List<AssetBundleInfo>> sceneNameDict = new Dictionary<string, List<AssetBundleInfo>>();
            List<List<AssetBundleInfo>> nonSceneBundlesDict = new List<List<AssetBundleInfo>>();

            //We do this in two seperate steps because I don't wanna minmax trying to get it in one right now.

            //Getting all unique scene names
            foreach (AssetBundleInfo bundleInfo in Instance.AssetBundleInfos)
            {
                List<string> sceneNames = bundleInfo.GetSceneNames();
                if (sceneNames.Count > 0)
                {
                    foreach (string sceneName in sceneNames)
                        if (!sceneNameDict.ContainsKey(sceneName))
                            sceneNameDict.Add(sceneName, new List<AssetBundleInfo>());
                }
                else
                    nonSceneBundlesDict.Add(new List<AssetBundleInfo> { bundleInfo });

            }


            foreach (KeyValuePair<string, List<AssetBundleInfo>> kvp in sceneNameDict)
            {
                UniqueSceneGroup sceneGroup = new UniqueSceneGroup(kvp.Key);
                uniqueSceneGroups.Add(sceneGroup);
                foreach (AssetBundleInfo info in kvp.Value)
                    sceneGroup.TryAdd(info, info.GetSceneNames());           
            }

            foreach (AssetBundleInfo info in Instance.AssetBundleInfos)
            {
                List<string> scenes = info.GetSceneNames();
                foreach (UniqueSceneGroup sceneGroup in uniqueSceneGroups)
                    if (sceneGroup.TryAdd(info, scenes))
                        break;
            }

            foreach (List<AssetBundleInfo> groupedInfos in uniqueSceneGroups.Select(s => s.AssetBundleInfosInGroup).Concat(nonSceneBundlesDict))
            {
                if (ValidatePotentialAssetBundleGroup(groupedInfos))
                {
                    AssetBundleGroup newGroup = new AssetBundleGroup(groupedInfos);
                    Instance.AssetBundleGroups.Add(newGroup);

                    foreach (KeyValuePair<(string, string), List<ParameterEvent<AssetBundleGroup>>> kvp in processedCallbacksDict)
                        foreach (AssetBundleInfo info in newGroup.GetAssetBundleInfos())
                        {
                            if (info.AssetBundleFilePath.Contains(kvp.Key.Item1) && info.AssetBundleFileName.Contains(kvp.Key.Item2))
                                foreach (ParameterEvent<AssetBundleGroup> groupEvent in kvp.Value)
                                    groupEvent.Invoke(newGroup);
                            break;
                        }

                    List<AssetBundle> allLoadedBundles = new List<AssetBundle>(AssetBundle.GetAllLoadedAssetBundles());
                    foreach (KeyValuePair<string, List<Action<AssetBundle>>> lethalBundleRequest in onLethalBundleLoadedRequestDict)
                        foreach (AssetBundle bundle in allLoadedBundles)
                        {
                            if (bundle != null && bundle.name == lethalBundleRequest.Key)
                            {
                                foreach (Action<AssetBundle> bundleEvent in lethalBundleRequest.Value)
                                    bundleEvent.Invoke(bundle);
                                break;
                            }
                        }


                        string log = "Generated New AssetBundleGroup, Contained BundleInfos Are,\n";
                    foreach (AssetBundleInfo bundleInfo in newGroup.GetAssetBundleInfos())
                        log += "\n" + bundleInfo.AssetBundleName;
                    DebugHelper.Log(log, DebugType.IAmBatby);
                }
            }

            //AssetBundle.UnloadAllAssetBundles(true);
            AllowLoading = true;
            OnBundlesFinishedProcessing.Invoke();

            foreach (AssetBundleInfo info in Instance.AssetBundleInfos)
                info.TryUnloadBundle();
        }

        private static bool ValidatePotentialAssetBundleGroup(List<AssetBundleInfo> assetBundleInfos)
        {
            if (assetBundleInfos.Count == 0)
                return (false);
            else
                return (true);


            if (assetBundleInfos.Count == 0)
            {
                Debug.Log("Could Not Make Grouping (No Matches)");
                return (false);
            }

            //LC specific validation because we don't want to load up any SelectableLevels's that don't have scenes with them
            int highestStandardSceneNamesCount = 0;
            int highestStreamingSceneNamesCount = 0;

            for (int i = 0; i < assetBundleInfos.Count; i++)
            {
                int sceneCount = assetBundleInfos[i].GetSceneNames().Count;
                if (assetBundleInfos[i].AssetBundleMode == AssetBundleType.Standard)
                {
                    if (sceneCount > highestStandardSceneNamesCount)
                        highestStandardSceneNamesCount = sceneCount;
                }
                else if (assetBundleInfos[i].AssetBundleMode == AssetBundleType.Streaming)
                {
                    if (sceneCount > highestStreamingSceneNamesCount)
                        highestStreamingSceneNamesCount = sceneCount;
                }
            }

            //TODO - LLL Scene Selection support
            /*if (highestStreamingSceneNamesCount != highestStandardSceneNamesCount)
            {
                Debug.Log("Could Not Make Grouping (Matching AssetBundleInfo's Have Inequal Scene Counts)");
                return (false);
            }*/

            //This validates under the assumption this system may support loading new scenes from things that are not SelectableLevels
            if (highestStreamingSceneNamesCount < highestStandardSceneNamesCount)
            {
                Debug.Log("Could Not Make Grouping (Standard Bundles Requesting More Scenes Than Streaming Bundles Have)");
                return (false);
            }

            return (true);
        }

        public static int GetAssetBundleLoadingCount()
        {
            int count = 0;
            foreach (AssetBundleInfo info in Instance.AssetBundleInfos)
                if (info.ActiveLoadingStatus == AssetBundleLoadingStatus.Loading)
                    count++;
            return (count);
        }

        private static List<AssetBundleInfo> GetAssetBundleInfos() => new List<AssetBundleInfo>(Instance.AssetBundleInfos);

        private static List<AssetBundleInfo> GetAssetBundleInfos(AssetBundleType modeFilter)
        {
            return (Instance.AssetBundleInfos.Where(a => a.AssetBundleMode == modeFilter)).ToList();
        }

        private static List<AssetBundleInfo> GetAssetBundleInfos(bool isLoadedFilter)
        {
            return (Instance.AssetBundleInfos.Where(a => a.IsAssetBundleLoaded == isLoadedFilter)).ToList();
        }
    }

    internal class UniqueSceneGroup
    {
        private List<string> scenesInGroup = new List<string>();
        internal List<AssetBundleInfo> AssetBundleInfosInGroup { get; private set; } = new List<AssetBundleInfo>();
        internal string UniqueSceneName { get; private set; } = string.Empty;

        internal UniqueSceneGroup(string newUniqueSceneName)
        {
            UniqueSceneName = newUniqueSceneName;
            scenesInGroup.Add(UniqueSceneName);
        }

        internal bool TryAdd(AssetBundleInfo info, List<string> infoScenes)
        {
            if (AssetBundleInfosInGroup.Contains(info)) return (true);

            if (infoScenes.Contains(UniqueSceneName))
            {
                Add(info, infoScenes);
                return (true);
            }
            else
            {
                foreach (string sceneName in infoScenes)
                    if (scenesInGroup.Contains(sceneName))
                    {
                        Add(info, infoScenes);
                        return (true);
                    }
            }
            return (false);
        }

        private void Add(AssetBundleInfo info, List<string> infoScenes)
        {
            if (!AssetBundleInfosInGroup.Contains(info))
                AssetBundleInfosInGroup.Add(info);
            foreach (string sceneName in infoScenes)
                if (!scenesInGroup.Contains(sceneName))
                    scenesInGroup.Add(sceneName);
        }
    }
}
