using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public struct AssetBundleInfo
    {
        public string DirectoryPath { get; private set; }
        private List<string> scenePathsInBundle = new List<string>();
        private List<string> sceneNamesInBundle = new List<string>();
        private AssetBundle assetBundle;

        public bool IsLoaded => (assetBundle != null);
        public bool IsSceneBundle => (sceneNamesInBundle.Count > 0);

        //We are taking in bundle for now but this might change once better
        public AssetBundleInfo(string directory, AssetBundle newBundle)
        {
            DirectoryPath = directory;
            assetBundle = newBundle;
            if (assetBundle != null)
                if (assetBundle.isStreamedSceneAssetBundle)
                    foreach (string scene in assetBundle.GetAllScenePaths())
                        scenePathsInBundle.Add(scene);


            foreach (string scene in scenePathsInBundle)
                sceneNamesInBundle.Add(scene.Substring(scene.LastIndexOf("/") + 1).Replace(".unity", string.Empty));
            foreach (string scene in sceneNamesInBundle)
                DebugHelper.Log("AssetBundleInfo Has Scene: " + scene, DebugType.User);
        }

        public AssetBundle LoadAndOrGetBundle()
        {
            if (IsLoaded)
                return (assetBundle);
            //else
            //load bundle stuffs

            return (null);
        }

        public bool ContainsScene(string scenePath)
        {
            return (sceneNamesInBundle.Contains(scenePath));
        }

        private void LoadBundle()
        {

        }

        private void UnloadBundle()
        {

        }
    }
}
