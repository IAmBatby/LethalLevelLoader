using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader.AssetBundles
{
    public enum AssetBundleGroupLoadedStatus { Unloaded, Partial, Loaded }
    public enum AssetBundleGroupLoadingStatus { None, Mixed, Loading, Unloading }

    public class AssetBundleGroup
    {
        public string GroupName { get; private set; } = string.Empty;
        private List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();

        public AssetBundleGroupLoadedStatus LoadedStatus
        {
            get
            {
                int loadedCount = 0;
                int unloadedCount = 0;
                foreach (AssetBundleInfo info in assetBundleInfos)
                {
                    if (info.IsAssetBundleLoaded == false)
                    {
                        unloadedCount++;
                        if (loadedCount > 0)
                            return (AssetBundleGroupLoadedStatus.Partial);
                    }

                    else
                    {
                        loadedCount++;
                        if (unloadedCount > 0)
                            return (AssetBundleGroupLoadedStatus.Partial);
                    }
                }
                if (loadedCount == assetBundleInfos.Count)
                    return (AssetBundleGroupLoadedStatus.Loaded);
                return (AssetBundleGroupLoadedStatus.Unloaded);
            }
        }

        public AssetBundleGroupLoadingStatus LoadingStatus
        {
            get
            {
                AssetBundleLoadingStatus returnStatus = AssetBundleLoadingStatus.None;
                foreach (AssetBundleInfo info in assetBundleInfos)
                {
                    if (info.ActiveLoadingStatus == AssetBundleLoadingStatus.Loading)
                    {
                        if (returnStatus == AssetBundleLoadingStatus.Unloading)
                            return (AssetBundleGroupLoadingStatus.Mixed);
                        else
                            returnStatus = AssetBundleLoadingStatus.Loading;
                    }
                    else if (info.ActiveLoadingStatus == AssetBundleLoadingStatus.Unloading)
                    {
                        if (returnStatus == AssetBundleLoadingStatus.Loading)
                            return (AssetBundleGroupLoadingStatus.Mixed);
                        else
                            returnStatus = AssetBundleLoadingStatus.Unloading;
                    }
                }
                if (returnStatus == AssetBundleLoadingStatus.Loading)
                    return (AssetBundleGroupLoadingStatus.Loading);
                else if (returnStatus == AssetBundleLoadingStatus.Unloading)
                    return (AssetBundleGroupLoadingStatus.Unloading);
                return (AssetBundleGroupLoadingStatus.None);
            }
        }

        public float ActiveProgress
        {
            get
            {
                float combinedProgress = 0f;
                float combinedTotal = 1f * assetBundleInfos.Count;
                foreach (AssetBundleInfo info in assetBundleInfos)
                    combinedProgress += info.ActiveProgress;
                return (Mathf.InverseLerp(0, combinedTotal, combinedProgress));
            }
        }

        public ExtendedEvent OnGroupLoaded = new ExtendedEvent();
        public ExtendedEvent OnGroupUnloaded = new ExtendedEvent();
        public ExtendedEvent OnGroupLoadStatusChanged = new ExtendedEvent();

        public AssetBundleGroup(AssetBundleInfo newInfo) => Initialize(newInfo);
        public AssetBundleGroup(params AssetBundleInfo[] newInfos) => Initialize(newInfos);
        public AssetBundleGroup(List<AssetBundleInfo> newInfos) => Initialize(newInfos.ToArray());

        private void Initialize(params AssetBundleInfo[] newInfos)
        {
            for (int i = 0; i < newInfos.Length; i++)
                if (newInfos[i] != null)
                    assetBundleInfos.Add(newInfos[i]);

            foreach (AssetBundleInfo info in assetBundleInfos)
            {
                info.OnBundleLoaded.AddListener(OnAssetBundleInfoLoadChanged);
                info.OnBundeUnloaded.AddListener(OnAssetBundleInfoLoadChanged);
            }
            GroupName = AssetBundleUtilities.GetDisplayName(assetBundleInfos);
        }

        private void OnAssetBundleInfoLoadChanged(AssetBundleInfo assetBundleInfo)
        {
            if (LoadedStatus == AssetBundleGroupLoadedStatus.Loaded)
                OnGroupLoaded.Invoke();
            else if (LoadedStatus == AssetBundleGroupLoadedStatus.Unloaded)
                OnGroupUnloaded.Invoke();
            else if (LoadedStatus == AssetBundleGroupLoadedStatus.Partial)
            {
                //If every hotreloadable bundle is unloaded we still consider that being unloaded(?)
                foreach (AssetBundleInfo info in assetBundleInfos)
                    if (info.IsHotReloadable && info.IsAssetBundleLoaded == true)
                        break;
                OnGroupUnloaded?.Invoke();
            }
            OnGroupLoadStatusChanged.Invoke();
        }

        //Might make this internal in the future
        internal List<AssetBundleInfo> GetAssetBundleInfos() => new List<AssetBundleInfo>(assetBundleInfos);

        public List<T> LoadAllAssets<T>() where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();

            for (int i = 0; i < assetBundleInfos.Count; i++)
                if (assetBundleInfos[i].AssetBundleMode == AssetBundleType.Standard)
                    returnList.AddRange(assetBundleInfos[i].LoadAllAssets<T>());

            return (returnList);
        }

        public void TryLoadGroup()
        {
            foreach (AssetBundleInfo info in assetBundleInfos)
                if (info.IsAssetBundleLoaded == false)
                    info.TryLoadBundle();
        }

        public void TryUnloadGroup()
        {
            foreach (AssetBundleInfo info in assetBundleInfos)
                if (info.IsAssetBundleLoaded == true)
                    info.TryUnloadBundle();
        }

        public bool ContainsAssetBundleFile(string fullFilePath)
        {
            for (int i = 0; i < assetBundleInfos.Count; i++)
                if (assetBundleInfos[i].AssetBundleFilePath.Equals(fullFilePath))
                    return (true);
            return (false);
        }

        public bool Contains(UnityEngine.Object unityObject)
        {
            for (int i = 0; i < assetBundleInfos.Count; i++)
                if (assetBundleInfos[i].Contains(unityObject))
                    return (true);
            return (false);
        }

        public bool Contains(string sceneNameOrPath)
        {
            for (int i = 0; i < assetBundleInfos.Count; i++)
                if (assetBundleInfos[i].Contains(sceneNameOrPath))
                    return (true);
            return (false);
        }

        public List<string> GetSceneNames()
        {
            List<string> returnList = new List<string>();
            foreach (AssetBundleInfo info in assetBundleInfos)
                returnList.AddRange(info.GetSceneNames());
            return (returnList);
        }
    }
}
