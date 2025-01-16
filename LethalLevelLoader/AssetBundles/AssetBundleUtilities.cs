using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader.AssetBundles
{
    public static class AssetBundleUtilities
    {
        public static string GetSceneName(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return ("Invalid Scene Path");
            if (!scenePath.Contains(".unity")) return ("Invalid Scene Path");
            if (scenePath.Contains("\\") && !scenePath.Contains("/"))
                return (scenePath.Substring(scenePath.LastIndexOf("\\") + 1).Replace(".unity", string.Empty));
            else if (scenePath.Contains("/") && !scenePath.Contains("\\"))
                return (scenePath.Substring(scenePath.LastIndexOf("/") + 1).Replace(".unity", string.Empty));
            return ("Invalid Scene Path");
        }

        public static string GetStopWatchTime(Stopwatch watch)
        {
            return (watch.Elapsed.TotalSeconds.ToString("F2") + "s");
        }

        //This is seperated all the way out here because the only LC specific part about the core of this system
        // is getting the scene names from the SelectableLevel's (and LLL's SceneSelection's in the future)
        internal static List<string> GetSceneNamesFromLoadedAssetBundle(AssetBundle assetBundle)
        {
            List<string> sceneNames = new List<string>();
            if (assetBundle == null) return (sceneNames);

            if (assetBundle.isStreamedSceneAssetBundle)
            {
                foreach (string scenePath in assetBundle.GetAllScenePaths())
                    sceneNames.Add(GetSceneName(scenePath));
            }
            else
            {
                    foreach (ExtendedMod mod in assetBundle.LoadAllAssets<ExtendedMod>())
                        foreach (ExtendedLevel level in mod.ExtendedLevels)
                        {
                            if (level.SelectableLevel != null && !string.IsNullOrEmpty(level.SelectableLevel.sceneName))
                                sceneNames.Add(level.SelectableLevel.sceneName);
                            foreach (StringWithRarity sceneSelection in level.SceneSelections)
                                if (!sceneNames.Contains(sceneSelection.Name))
                                    sceneNames.Add(sceneSelection.Name);
                        }


                foreach (ExtendedLevel level in assetBundle.LoadAllAssets<ExtendedLevel>())
                {
                    foreach (StringWithRarity sceneSelection in level.SceneSelections)
                        if (!sceneNames.Contains(sceneSelection.Name))
                            sceneNames.Add(sceneSelection.Name);
                    if (level.SelectableLevel != null && !string.IsNullOrEmpty(level.SelectableLevel.sceneName))
                        sceneNames.Add(level.SelectableLevel.sceneName);
                }

            }

            return (sceneNames);
        }

        //Extremely arbitary but that's fine for display purposes
        internal static string GetDisplayName(List<AssetBundleInfo> bundleInfos)
        {
            if (bundleInfos.Count == 0) return (string.Empty);
            AssetBundleInfo mostImportantBundle = null;
            foreach (AssetBundleInfo bundleInfo in bundleInfos)
            {
                if (mostImportantBundle == null)
                    mostImportantBundle = bundleInfo;
                else if (bundleInfo.GetSceneNames().Count > mostImportantBundle.GetSceneNames().Count)
                    mostImportantBundle = bundleInfo;
                else if (bundleInfo.GetSceneNames().Count == mostImportantBundle.GetSceneNames().Count)
                    if (mostImportantBundle.AssetBundleMode == AssetBundleType.Streaming)
                        if (bundleInfo.AssetBundleMode == AssetBundleType.Standard)
                            mostImportantBundle = bundleInfo;
            }

            string returnName = mostImportantBundle.AssetBundleName;

            if (returnName.Contains('.'))
                returnName = returnName.TrimEnd('.');
            char[] chars = returnName.ToCharArray();
            chars[0] = char.ToUpperInvariant(chars[0]);

            return (new string(chars));
        }

        //Awful fix later
        internal static string GetLoadingPercentage(AssetBundleGroup group) => GetLoadingPercentage(group.ActiveProgress);
        internal static string GetLoadingPercentage(AssetBundleInfo info) => GetLoadingPercentage(info.ActiveProgress);
        internal static string GetLoadingPercentage(float progress)
        {
            string loadingProgressText = progress.ToString("F2");
            if (loadingProgressText.Contains("0."))
                loadingProgressText = loadingProgressText.Replace("0.", "0");
            else if (loadingProgressText.Contains("."))
                loadingProgressText = loadingProgressText.Replace(".", string.Empty);
            if (loadingProgressText == "00")
                loadingProgressText = "000";
            return (loadingProgressText + "%");
        }
    }
}
