using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using Debug = UnityEngine.Debug;

namespace LethalLevelLoader.AssetBundles
{
    public enum AssetBundleType { Unknown, Standard, Streaming }
    public enum AssetBundleLoadingStatus { None, Loading, Unloading }

    public class AssetBundleInfo
    {
        private bool hasInitialized;
        private AssetBundle assetBundle;
        private MonoBehaviour coroutineHandler;
        private AssetBundleCreateRequest activeLoadRequest;
        private AssetBundleUnloadOperation activeUnloadRequest;

        private Stopwatch bundleLoadStopwatch;
        private Stopwatch bundleUnloadStopwatch;

        public string LastLoadTime => AssetBundleUtilities.GetStopWatchTime(bundleLoadStopwatch);
        public float LastTimeLoaded { get; private set; }
        public string LastUnloadTime => AssetBundleUtilities.GetStopWatchTime(bundleUnloadStopwatch);
        public float LastTimeUnloaded { get; private set; }



        private List<string> allAssetPaths = new List<string>();
        private List<string> streamingBundleScenePaths = new List<string>();
        private List<string> sceneNames = new List<string>();

        public string AssetBundleName { get; private set; } = "UNKNOWN";
        public AssetBundleType AssetBundleMode { get; private set; }
        public bool IsAssetBundleLoaded => (assetBundle != null);

        public string AssetBundleFileName { get; private set; } = "UNKNOWN";
        public string AssetBundleFilePath { get; private set; } = string.Empty;

        public bool IsHotReloadable { get; set; }

        public float ActiveProgress
        {
            get
            {
                if (activeLoadRequest != null)
                    return (activeLoadRequest.progress);
                else if (activeUnloadRequest != null)
                    return (activeUnloadRequest.progress);
                else if (IsAssetBundleLoaded)
                    return (1f);
                else
                    return (0f);
            }
        }

        public AssetBundleLoadingStatus ActiveLoadingStatus
        {
            get
            {
                if (activeLoadRequest != null)
                    return (AssetBundleLoadingStatus.Loading);
                else if (activeUnloadRequest != null)
                    return (AssetBundleLoadingStatus.Unloading);
                return (AssetBundleLoadingStatus.None);
            }
        }

        public ExtendedEvent<AssetBundleInfo> OnBundleLoaded = new ExtendedEvent<AssetBundleInfo>();
        public ExtendedEvent<AssetBundleInfo> OnBundeUnloaded = new ExtendedEvent<AssetBundleInfo>();

        public AssetBundleInfo(MonoBehaviour newCoroutineHandler, string filePath)
        {
            coroutineHandler = newCoroutineHandler;
            AssetBundleFilePath = filePath;
            if (filePath.Contains("\\"))
                AssetBundleFileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            bundleLoadStopwatch = new Stopwatch();
            bundleUnloadStopwatch = new Stopwatch();
        }

        public void Initialize()
        {
            if (hasInitialized) return;
            hasInitialized = true;

            AssetBundleName = assetBundle.name;
            sceneNames = AssetBundleUtilities.GetSceneNamesFromLoadedAssetBundle(assetBundle);
            if (assetBundle.isStreamedSceneAssetBundle)
            {
                AssetBundleMode = AssetBundleType.Streaming;
                streamingBundleScenePaths = new List<string>(assetBundle.GetAllScenePaths());
                allAssetPaths = new List<string>(streamingBundleScenePaths);
            }
            else
            {
                AssetBundleMode = AssetBundleType.Standard;
                allAssetPaths = new List<string>(assetBundle.GetAllAssetNames());
            }
        }

        public bool TryLoadBundle()
        {
            if (IsAssetBundleLoaded == true)
            {
                OnBundleLoaded.Invoke(this); //Feels a little strange but if something requests a bundle to be loaded and expects this event to fire in response we wanna fire it in the event it's already loaded. (might change later) 
                return (true);
            }
            else if (IsAssetBundleLoaded == false && activeLoadRequest == null)
            {
                coroutineHandler.StartCoroutine(LoadBundleRequest());
                return (true);
            }
            else
            {
                DebugHelper.Log("Failed To Load: " + AssetBundleFileName, DebugType.User);
                return (false);
            }
        }

        public bool TryUnloadBundle()
        {
            if (IsHotReloadable == false) return (false);
            else if (IsAssetBundleLoaded == false)
            {
                OnBundeUnloaded.Invoke(this);  //Feels a little strange but if something requests a bundle to be unloaded and expects this event to fire in response we wanna fire it in the event it's already unloaded. (might change later) 
                return (true);
            }
            else if (activeUnloadRequest == null)
            {
                coroutineHandler.StartCoroutine(UnloadBundleRequest());
                return (true);
            }
            else
            {
                DebugHelper.Log("Failed To Unload: " + AssetBundleFileName, DebugType.User);
                return (false);
            }
        }

        private IEnumerator LoadBundleRequest()
        {
            bundleLoadStopwatch = Stopwatch.StartNew();
            string combinedPath = Path.Combine(Application.streamingAssetsPath, AssetBundleFilePath);
            activeLoadRequest = AssetBundle.LoadFromFileAsync(combinedPath);
            yield return activeLoadRequest;
            if (assetBundle != null || (activeLoadRequest.isDone && activeLoadRequest.assetBundle != null))
            {
                assetBundle = activeLoadRequest.assetBundle;
                if (hasInitialized == false)
                    Initialize();
                activeLoadRequest = null;
                bundleLoadStopwatch.Stop();
                LastTimeLoaded = Time.time;
                DebugHelper.Log(AssetBundleFileName + " Loaded (" + LastLoadTime + ")!", DebugType.User);
                OnBundleLoaded.Invoke(this);
            }
            else
                DebugHelper.LogError("AssetBundleInfo: " + AssetBundleFileName + " failed to load.", DebugType.User);
        }

        private IEnumerator UnloadBundleRequest()
        {
            bundleUnloadStopwatch = Stopwatch.StartNew();
            yield return new WaitForSeconds(0.01f); //Might remove later but stopped unity freeze when you tried to load and unload a bundle on the same frame (Confirmed Unity bug on our version)
            activeUnloadRequest = assetBundle.UnloadAsync(true);
            yield return activeUnloadRequest;
            if (activeUnloadRequest.isDone)
            {
                UnityEngine.Object.Destroy(assetBundle);
                assetBundle = null; // I think we need to do this so it isn't deemed missing (?)
                activeUnloadRequest = null;
                bundleUnloadStopwatch.Stop();
                LastTimeUnloaded = Time.time;
                DebugHelper.Log(AssetBundleFileName + " Unloaded (" + LastUnloadTime + ")", DebugType.User);
                OnBundeUnloaded.Invoke(this);
            }
            
        }

        ////////// AssetBundle Middle-Man'd Functions //////////

        public List<T> LoadAllAssets<T>() where T : UnityEngine.Object
        {
            if (AssetBundleMode == AssetBundleType.Unknown || AssetBundleMode == AssetBundleType.Streaming)
                return (new List<T>());

            if (IsAssetBundleLoaded == false || assetBundle == null)
                return (new List<T>());

            return (new List<T>(assetBundle.LoadAllAssets<T>()));
        }

        public List<string> GetSceneNames() => new List<string>(sceneNames);

        public bool Contains(string sceneNameOrPath)
        {
            if (IsAssetBundleLoaded == false) return (false);
            if (AssetBundleMode == AssetBundleType.Standard) return (false);
            if (string.IsNullOrEmpty(sceneNameOrPath)) return (false);
            return (streamingBundleScenePaths.Contains(sceneNameOrPath) || sceneNames.Contains(sceneNameOrPath));
        }

        public bool Contains(UnityEngine.Object unityObject)
        {
            if (IsAssetBundleLoaded == false) return (false);
            if (AssetBundleMode == AssetBundleType.Streaming) return (false);
            if (unityObject == null || unityObject.name == null) return (false);
            return (assetBundle.Contains(unityObject.name));
        }
    }
}
