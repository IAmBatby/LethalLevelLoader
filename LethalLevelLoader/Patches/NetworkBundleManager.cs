using LethalLevelLoader.AssetBundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    internal class NetworkBundleManager : NetworkBehaviour
    {
        public static GameObject networkingManagerPrefab;
        private static NetworkBundleManager _instance;
        public static NetworkBundleManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = UnityEngine.Object.FindObjectOfType<NetworkBundleManager>();
                if (_instance == null)
                    DebugHelper.LogError("NetworkBundleManager Could Not Be Found! Returning Null!", DebugType.User);
                return _instance;
            }
            set { _instance = value; }
        }
        public static NetworkManager networkManager;

        public List<NetworkSceneInfo> networkSceneInfos;

        internal Dictionary<string, List<AssetBundleGroup>> assetBundleGroupSceneDict = new Dictionary<string, List<AssetBundleGroup>>();
        internal List<AssetBundleGroup> currentRouteRequestedBundles = new List<AssetBundleGroup>();
        internal ExtendedLevel currentRouteRequestor;

        private NetworkVariable<bool> allowedToLoadLevel = new NetworkVariable<bool>();
        internal static bool AllowedToLoadLevel => Instance == null ? false : Instance.allowedToLoadLevel.Value;

        private Dictionary<ulong, bool> playersReadyDict = new Dictionary<ulong, bool>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = "NetworkBundleManager";
            DebugHelper.Log("NetworkBundleManger Has Spawned!", DebugType.IAmBatby);
            Instance = this;
            if (Plugin.IsSetupComplete == true)
            {
                GenerateSceneDict();
                GenerateAssetBundleGroupDict();
                LogStuff();
            }
            else
            {
                Plugin.onSetupComplete += GenerateSceneDict;
                Plugin.onSetupComplete += GenerateAssetBundleGroupDict;
                Plugin.onSetupComplete += LogStuff;
                Plugin.onSetupComplete += Refresh;
            }
            AssetBundles.AssetBundleLoader.OnBundleLoaded.AddListener(Instance.TryRefreshLoadedStatus);
            AssetBundles.AssetBundleLoader.OnBundleUnloaded.AddListener(Instance.TryRefreshLoadedStatus);
        }

        internal void Refresh()
        {
            if (IsHost == false) return;
            allowedToLoadLevel.Value = true;
            OnRouteChanged();
        }

        internal void OnRouteChanged()
        {
            if (currentRouteRequestor == LevelManager.CurrentExtendedLevel)
                return;

            foreach (AssetBundleGroup bundleGroup in currentRouteRequestedBundles)
                bundleGroup.TryUnloadGroup();

            currentRouteRequestedBundles.Clear();

            currentRouteRequestor = LevelManager.CurrentExtendedLevel;
            List<string> levelScenes = currentRouteRequestor.SceneSelections.Select(s => s.Name).ToList();

            foreach (string levelScene in levelScenes)
                if (assetBundleGroupSceneDict.TryGetValue(levelScene, out List<AssetBundleGroup> groups))
                    foreach (AssetBundleGroup group in groups)
                        if (!currentRouteRequestedBundles.Contains(group))
                            currentRouteRequestedBundles.Add(group);

            foreach (AssetBundleGroup bundleGroup in currentRouteRequestedBundles)
                bundleGroup.TryLoadGroup();

            ResetLoadedBundlesStatus();
        }

        private void TryRefreshLoadedStatus()
        {
            if (AllowedToLoadLevel == false)
                UpdateLoadedStatus();
        }

        internal void GenerateSceneDict()
        {
            networkSceneInfos = new List<NetworkSceneInfo>();
            Dictionary<int, string> levelSceneDict = NetworkScenePatcher.GetLevelSceneDict();
            List<string> scenePaths = new List<string>(levelSceneDict.Values);
            for (int i = 0; i < levelSceneDict.Count; i++)
                if (scenePaths.Count > i)
                    networkSceneInfos.Add(new NetworkSceneInfo(i, scenePaths[i]));
        }

        internal void GenerateAssetBundleGroupDict()
        {
            assetBundleGroupSceneDict = new Dictionary<string, List<AssetBundleGroup>>();
            foreach (AssetBundleGroup group in AssetBundles.AssetBundleLoader.Instance.AssetBundleGroups)
                foreach (string groupSceneName in group.GetSceneNames())
                {
                    if (assetBundleGroupSceneDict.TryGetValue(groupSceneName, out List<AssetBundleGroup> bundleList))
                    {
                        if (!bundleList.Contains(group))
                            bundleList.Add(group);
                    }
                    else
                        assetBundleGroupSceneDict.Add(groupSceneName, new List<AssetBundleGroup> { group });
                }
        }

        internal void ResetLoadedBundlesStatus()
        {
            if (IsHost == false) return;

            if (currentRouteRequestedBundles.Count == 0)
            {
                allowedToLoadLevel.Value = true;
                return;
            }

            DebugHelper.Log("Refeshing Loaded Bundles Status!", DebugType.User);

            playersReadyDict.Clear();

            foreach (ulong fullyLoadedPlayer in Patches.StartOfRound.fullyLoadedPlayers)
                playersReadyDict.Add(fullyLoadedPlayer, false);

            RefreshLoadedBundlesStatus();
        }

        internal void RefreshLoadedBundlesStatus()
        {
            if (Plugin.IsSetupComplete == false) return;

            if (IsServer || IsHost)
                UpdateLoadedStatusClientRpc();

            DebugHelper.Log("AllowedToLoadLevel: " + AllowedToLoadLevel, DebugType.User);
        }

        [ClientRpc]
        internal void UpdateLoadedStatusClientRpc()
        {
            UpdateLoadedStatus();
        }

        internal void UpdateLoadedStatus()
        {
            if (Plugin.IsSetupComplete == false) return;
            bool loadedStatus = true;
            foreach (AssetBundleGroup routeGroup in currentRouteRequestedBundles)
                if (routeGroup.LoadedStatus != AssetBundleGroupLoadedStatus.Loaded)
                    loadedStatus = false;
            DebugHelper.Log("Sending LoadedStatus: " + loadedStatus + " To Server!", DebugType.User);
            UpdateLoadedStatusServerRpc(NetworkManager.LocalClientId, loadedStatus);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void UpdateLoadedStatusServerRpc(ulong clientID, bool loadedStatus)
        {
            playersReadyDict[clientID] = loadedStatus;
            int progress = 0;
            foreach (KeyValuePair<ulong, bool> status in playersReadyDict)
                if (status.Value == true)
                    progress++;

            allowedToLoadLevel.Value = progress == playersReadyDict.Count;

            DebugHelper.Log("LoadedStatus Is Currently: (" + progress + " / " + playersReadyDict.Count + ")", DebugType.User);
        }

        internal void LogStuff()
        {
            DebugHelper.Log("NetworkBundleManager Spawned!", DebugType.IAmBatby);

            DebugHelper.Log("NetworkSceneInfos!", DebugType.IAmBatby);
            foreach (NetworkSceneInfo info in networkSceneInfos)
                DebugHelper.Log("Level Scene Index: " + info.LevelSceneIndex + ", Scene Index: " + info.SceneIndex + ", Scene Path: " + info.LevelScenePath + ", Origin: " + info.Origin + ", IsLoaded: " + info.IsLoaded, DebugType.IAmBatby);

            DebugHelper.Log("AssetBundleInfos!", DebugType.User);
            foreach (AssetBundleInfo bundleInfo in AssetBundleLoader.AssetBundleInfos)
                DebugHelper.Log("Path: " + bundleInfo.DirectoryPath + ", IsLoaded: " + bundleInfo.IsLoaded + ", IsSceneBundle: " + bundleInfo.IsSceneBundle, DebugType.IAmBatby);
        }

    }
}
