using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace LethalLevelLoader
{
    public class AssetBundleManager : MonoBehaviour
    {
        internal static DirectoryInfo pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;
        public const string specifiedFileExtension = "*.lethalbundle";

        public void Load()
        {
            StartCoroutine(LoadAllAssetBundles());
        }

        internal IEnumerator LoadAllAssetBundles()
        {
            List<LoadedBundleInfo> loadedBundles = new List<LoadedBundleInfo>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
            {
                //BundleType newBundleType;

                FileInfo fileInfo = new FileInfo(file);
                string bundleString = Path.Combine(Application.streamingAssetsPath, file);
                AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromFileAsync(bundleString);
                yield return newBundleRequest;
                AssetBundle newBundle = newBundleRequest.assetBundle;

                if (newBundle != null )
                {
                    BundleType newBundleType;

                    if (newBundle.isStreamedSceneAssetBundle)
                        newBundleType = BundleType.Scene;
                    else if (newBundle.LoadAllAssets<SceneBundleManifest>() != null)
                        newBundleType = BundleType.SceneData;
                    else
                        newBundleType = BundleType.Default;

                    loadedBundles.Add(new LoadedBundleInfo(newBundle, newBundleType, newBundle.name, bundleString));
                    DebugHelper.Log("Processed LethalBundle: " + newBundle.name, DebugType.User);
                }
            }
            stopWatch.Stop();
            DebugHelper.Log("Processed #" + loadedBundles.Count + " LoadedBundleInfo's (Time Elapsed: " + ConvertTime(stopWatch) + ")", DebugType.User);
        }

        internal void ProcessLoadedBundleInfos(List<LoadedBundleInfo> loadedBundles)
        {
            Dictionary<string, LethalBundle> allLethalBundlesDict = new Dictionary<string, LethalBundle>();

            foreach (LoadedBundleInfo loadedBundle in loadedBundles)
            {
                BundleInfo bundleInfo = new BundleInfo(loadedBundle.bundleName, loadedBundle.bundlePath, loadedBundle.bundleType);
                if (loadedBundle.bundleType == BundleType.Scene)
                {
                    foreach (string scenePath in loadedBundle.assetBundle.GetAllScenePaths())
                    {
                        string sceneName = GetSceneName(scenePath);
                        if (allLethalBundlesDict.TryGetValue(sceneName, out LethalBundle bundle))
                            bundle.AddBundleInfo(bundleInfo);
                        else
                            allLethalBundlesDict.Add(sceneName, new LethalBundle(bundleInfo));
                    }
                }
                else if (loadedBundle.bundleType == BundleType.Default)
                {

                }
            }
        }

        internal static string GetSceneName(string scenePath) => scenePath.Substring(scenePath.LastIndexOf('/') + 1).Replace(".unity", "");

        internal string ConvertTime(Stopwatch stopWatch) => ((int) stopWatch.Elapsed.TotalMinutes + ":" + (int) stopWatch.Elapsed.TotalSeconds);
    }

    public struct LoadedBundleInfo
    {
        public AssetBundle assetBundle;
        public BundleType bundleType;
        public string bundleName;
        public string bundlePath;

        public LoadedBundleInfo(AssetBundle newBundle, BundleType newType, string newBundleName, string newBundlePath)
        {
            assetBundle = newBundle;
            bundleType = newType;
            bundleName = newBundleName;
            bundlePath = newBundlePath;
        }
    }
}
