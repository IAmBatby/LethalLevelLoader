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
        
        //internal List<AssetBundleGroup> currentRouteRequestedBundles = new List<AssetBundleGroup>();
        internal ExtendedLevel currentRouteRequestor;

        private NetworkVariable<bool> allowedToLoadLevel = new NetworkVariable<bool>();
        internal static bool AllowedToLoadLevel => Instance == null ? false : Instance.allowedToLoadLevel.Value;

        //private Dictionary<ulong, bool> playersReadyDict = new Dictionary<ulong, bool>();

        private NetworkList<bool> playersLoadStatus = new NetworkList<bool>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = "NetworkBundleManager";
            DebugHelper.Log("NetworkBundleManger Has Spawned!", DebugType.IAmBatby);
            Instance = this;

            if (Plugin.IsLobbyInitialized == true)
                Initialize();
            else
            {
                Plugin.onLobbyInitialized -= Initialize;
                Plugin.onLobbyInitialized += Initialize;
            }
            AssetBundles.AssetBundleLoader.OnBundleLoaded.AddListener(Instance.RefreshLoadStatus);
            AssetBundles.AssetBundleLoader.OnBundleUnloaded.AddListener(Instance.RefreshLoadStatus);
        }

        //This should run anytime the client joins a lobby
        private void Initialize()
        {
            DebugHelper.Log("NetworkBundleManager Initializing.", DebugType.User);
            GenerateSceneDict();
            GenerateAssetBundleGroupDict();
            Refresh();
        }

        //Called on Plugin.onSetupComplete
        //Called by StartOfRound.ChangeLevel.Postfix
        internal void Refresh()
        {
            List<AssetBundleGroup> newGroups = GetRouteGroups(LevelManager.CurrentExtendedLevel);
            List<AssetBundleGroup> previousGroups = new List<AssetBundleGroup>();
            if (currentRouteRequestor != null)
                previousGroups = GetRouteGroups(currentRouteRequestor);

            if (currentRouteRequestor != LevelManager.CurrentExtendedLevel)
                foreach (AssetBundleGroup bundleGroup in previousGroups)
                    if (!newGroups.Contains(bundleGroup))
                        bundleGroup.TryUnloadGroup();

            currentRouteRequestor = LevelManager.CurrentExtendedLevel;

            foreach (AssetBundleGroup bundleGroup in newGroups)
                if (!previousGroups.Contains(bundleGroup))
                    bundleGroup.TryLoadGroup();

            if (IsServer)
                RequestLoadStatusRefreshServerRpc();
        }

        //Called by StartOfRound.OnClientConnect.Postfix
        //Called by StartOfRound.OnClientDisconnect.Postfix
        internal void OnClientsChangedRefresh()
        {
            if (!IsServer) return;
            RequestLoadStatusRefreshServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        internal void RequestLoadStatusRefreshServerRpc()
        {
            DebugHelper.Log("Refeshing Loaded Bundles Status!", DebugType.User);
            playersLoadStatus.Clear();
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
                playersLoadStatus.Add(false);
            RequestLoadStatusRefreshClientRpc();
        }

        [ClientRpc]
        private void RequestLoadStatusRefreshClientRpc()
        {
            RefreshLoadStatus();
        }

        //Called by RequestLoadStatusRefreshClientRpc
        //Called by OnBundleLoaded
        //Called by OnBundleUnloaded
        private void RefreshLoadStatus()
        {
            bool loadedStatus = true;
            foreach (AssetBundleGroup routeGroup in GetRouteGroups(LevelManager.CurrentExtendedLevel))
                if (routeGroup.LoadedStatus != AssetBundleGroupLoadedStatus.Loaded)
                {
                    loadedStatus = false;
                    if (routeGroup.LoadingStatus != AssetBundleGroupLoadingStatus.Loading)
                        routeGroup.TryLoadGroup();
                }
            DebugHelper.Log("Sending LoadedStatus: " + loadedStatus + " To Server!", DebugType.User);
            SetLoadedStatusServerRpc(NetworkManager.LocalClientId, loadedStatus);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetLoadedStatusServerRpc(ulong clientID, bool status)
        {
            int index = NetworkManager.ConnectedClientsIds.ToList().IndexOf(clientID);
            if (playersLoadStatus.Count <= index)
            {
                DebugHelper.LogError("Tried To Set LoadedStatus When List Is Invalid (ClientID: " + clientID + ", Index: " + index + "), Resetting.", DebugType.User);
                RequestLoadStatusRefreshServerRpc();
                return;
            }

            playersLoadStatus[index] = status;
            int progress = 0;
            foreach (bool loadStatus in playersLoadStatus)
                if (loadStatus == true)
                    progress++;
            allowedToLoadLevel.Value = (progress == playersLoadStatus.Count);
            DebugHelper.Log("LoadedStatus Is Currently: (" + progress + " / " + playersLoadStatus.Count + ")", DebugType.User);
        }

        private List<AssetBundleGroup> GetRouteGroups(ExtendedLevel route)
        {
            List<AssetBundleGroup> returnList = new List<AssetBundleGroup>();
            if (route == null)
                return (returnList);
            List<string> levelScenes = route.SceneSelections.Select(s => s.Name).ToList();
            foreach (string levelScene in levelScenes)
                if (assetBundleGroupSceneDict.TryGetValue(levelScene, out List<AssetBundleGroup> groups))
                    foreach (AssetBundleGroup group in groups)
                        if (!returnList.Contains(group))
                            returnList.Add(group);
            return (returnList);
        }

        private void GenerateSceneDict()
        {
            networkSceneInfos = new List<NetworkSceneInfo>();
            Dictionary<int, string> levelSceneDict = NetworkScenePatcher.GetLevelSceneDict();
            List<string> scenePaths = new List<string>(levelSceneDict.Values);
            for (int i = 0; i < levelSceneDict.Count; i++)
                if (scenePaths.Count > i)
                    networkSceneInfos.Add(new NetworkSceneInfo(i, scenePaths[i]));
        }

        private void GenerateAssetBundleGroupDict()
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

        /*
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
        */

    }
}
