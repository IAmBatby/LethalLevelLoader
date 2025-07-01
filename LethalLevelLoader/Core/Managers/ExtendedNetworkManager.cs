using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using DunGen.Graph;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Reflection;
using Zeekerss.Core.Singletons;

namespace LethalLevelLoader
{
    public class ExtendedNetworkManager : NetworkSingleton<ExtendedNetworkManager>
    {
        public static ExtendedNetworkManager Instance => NetworkInstance as ExtendedNetworkManager;
        private static Dictionary<GameObject, NetworkPrefab> NetworkPrefabRegistry = new Dictionary<GameObject, NetworkPrefab>();
        private static Dictionary<string, GameObject> VanillaNetworkPrefabNameDict = new Dictionary<string, GameObject>();
        private static List<GameObject> queuedInternalNetworkPrefabs = new List<GameObject>();
        private static Dictionary<ExtendedMod, List<GameObject>> networkPrefabCollections = new Dictionary<ExtendedMod, List<GameObject>>();
        public static bool networkHasStarted;

        private static List<NetworkSingleton> queuedNetworkSingletonSpawns = new List<NetworkSingleton>();

        public static bool IsLobbyNetworkInitialized { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            DebugHelper.Log("Inherited Init Is Working Yippeeeee", DebugType.User);
            Events.OnCurrentStateChanged.AddListener(OnGameStateChanged);
        }

        private static void OnGameStateChanged(GameStates state)
        {
            if (state == GameStates.MainMenu)
                IsLobbyNetworkInitialized = false;
        }

        internal static T CreateAndRegisterNetworkSingleton<T>(Type realType,  string name, bool dontDestroyWithOwner = false, bool sceneMigration = true, bool destroyWithScene = true) where T : NetworkSingleton
        {
            var prefab = Utilities.CreateNetworkPrefab<T>(realType, name, dontDestroyWithOwner, sceneMigration, destroyWithScene);
            queuedNetworkSingletonSpawns.Add(prefab);
            return (prefab);
        }

        static int activeNetworkSingletonSpawnCounter;

        internal static void SpawnNetworkSingletons()
        {
            activeNetworkSingletonSpawnCounter = queuedNetworkSingletonSpawns.Count;
            if (!NetworkManagerInstance.IsServer) return;
            DebugHelper.Log("Spawning: " + activeNetworkSingletonSpawnCounter + " NetworkSingletons!", DebugType.User);
            foreach (NetworkSingleton singletonPrefab in queuedNetworkSingletonSpawns)
            {
                DebugHelper.Log("Spawning NetworkSingleton: " + singletonPrefab.name, DebugType.User);
                Instantiate(singletonPrefab).GetComponent<NetworkObject>().Spawn(true); //MAKE NOT TRUE LATER
            }
        }

        internal static void OnNetworkSingletonSpawned(NetworkSingleton singleton)
        {
            DebugHelper.Log("NetworkSingleton: " + singleton.name + " Has Spawned!", DebugType.User);
            activeNetworkSingletonSpawnCounter--;
            if (activeNetworkSingletonSpawnCounter == 0)
                OnNetworkSingletonsSpawned();
        }

        internal static void OnNetworkSingletonsSpawned()
        {
            DebugHelper.Log("All Registered NetworkSingletons Have Spawned!", DebugType.User);
            IsLobbyNetworkInitialized = true;
        }

        //private static Dictionary<Type, NetworkSingleton> networkSingletonNetworkPrefabs = new Dictionary<Type, NetworkSingleton>();
        //private static Dictionary<NetworkSingleton, NetworkSingleton> networkSingletonSpawnedPrefabs = new Dictionary<NetworkSingleton, NetworkSingleton>();
        /*
        public static void SpawnNetworkSingleton<T>(T networkSingletonPrefab, bool destroyWithScene = false) where T : NetworkSingleton<T>
        {
            if (networkSingletonNetworkPrefabs.ContainsKey(typeof(T))) return;
            networkSingletonNetworkPrefabs.Add(typeof(T), networkSingletonPrefab); //Gotta add on clients
            if (!networkManager.IsServer) return;
            Instantiate(networkSingletonPrefab).GetComponent<NetworkObject>().Spawn(destroyWithScene);
        }

        internal static void OnNetworkSingletonSpawn<T>(T networkSingletonInstance) where T : NetworkSingleton<T>
        {
            if (networkSingletonNetworkPrefabs.TryGetValue(typeof(T), out NetworkSingleton prefab))
                networkSingletonSpawnedPrefabs.Add(prefab, networkSingletonInstance);
        }

        //Needs work
        internal static void OnNetworkSingletonDespawn<T>(T networkSingletonInstance) where T : NetworkSingleton<T>
        {
            if (networkSingletonSpawnedPrefabs.ContainsKey(networkSingletonNetworkPrefabs[typeof(T)]))
                networkSingletonSpawnedPrefabs.Remove(networkSingletonNetworkPrefabs[typeof(T)]);
        }*/

        protected override void OnNetworkSingletonSpawn()
        {
            gameObject.name = "LethalLevelLoaderNetworkManager";
            DebugHelper.Log("LethalLevelLoaderNetworkManager Spawned.", DebugType.User);
        }

        public static void TryRefreshWeather()
        {
            if (IsSpawnedAndIntialized)
                Instance.GetUpdatedLevelCurrentWeatherServerRpc();
        }

        [ServerRpc]
        public void GetRandomExtendedDungeonFlowServerRpc()
        {
            DebugHelper.Log("Getting Random DungeonFlows!", DebugType.User);

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonManager.GetValidExtendedDungeonFlows(LevelManager.CurrentExtendedLevel, debugResults: true);

            //List<string> dungeonFlowNames = new List<string>();
            List<StringContainer> dungeonFlowNames = new List<StringContainer>();
            List<int> rarities = new List<int>();

            if (availableExtendedFlowsList.Count == 0)
            {
                DebugHelper.LogError("No ExtendedDungeonFlow's could be found! This should only happen if the Host's requireMatchesOnAllDungeonFlows is set to true!", DebugType.User);
                DebugHelper.LogError("Loading Facility DungeonFlow to prevent infinite loading!", DebugType.User);
                StringContainer newStringContainer = new StringContainer();
                newStringContainer.SomeText = PatchedContent.ExtendedDungeonFlows[0].DungeonFlow.name;
                dungeonFlowNames.Add(newStringContainer);
                rarities.Add(300);
            }
            else
            {
                List<DungeonFlow> dungeonFlowTypes = Patches.RoundManager.GetDungeonFlows();
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in availableExtendedFlowsList)
                {
                    StringContainer newStringContainer = new StringContainer();
                    newStringContainer.SomeText = dungeonFlowTypes[dungeonFlowTypes.IndexOf(extendedDungeonFlowWithRarity.extendedDungeonFlow.DungeonFlow)].name;
                    dungeonFlowNames.Add(newStringContainer);

                    rarities.Add(extendedDungeonFlowWithRarity.rarity);
                }
            }

            SetRandomExtendedDungeonFlowClientRpc(dungeonFlowNames.ToArray(), rarities.ToArray());
        }

        [ServerRpc]
        private void GetUpdatedLevelCurrentWeatherServerRpc()
        {
            List<StringContainer> levelNames = new List<StringContainer>();
            List<LevelWeatherType> weatherTypes = new List<LevelWeatherType>();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                StringContainer stringContainer = new StringContainer();
                stringContainer.SomeText = extendedLevel.name;
                levelNames.Add(stringContainer);
                weatherTypes.Add(extendedLevel.SelectableLevel.currentWeather);
            }

            SetUpdatedLevelCurrentWeatherClientRpc(levelNames.ToArray(), weatherTypes.ToArray());
        }

        [ClientRpc]
        public void SetUpdatedLevelCurrentWeatherClientRpc(StringContainer[] levelNames, LevelWeatherType[] weatherTypes)
        {
            Dictionary<ExtendedLevel, LevelWeatherType> syncedLevelCurrentWeathers = new Dictionary<ExtendedLevel, LevelWeatherType>();

            for (int i = 0; i < levelNames.Length; i++)
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                    if (levelNames[i].SomeText == extendedLevel.name)
                        syncedLevelCurrentWeathers.Add(extendedLevel, weatherTypes[i]);

            foreach (KeyValuePair<ExtendedLevel, LevelWeatherType> syncedWeather in syncedLevelCurrentWeathers)
            {
                if (syncedWeather.Key.SelectableLevel.currentWeather != syncedWeather.Value)
                {
                    DebugHelper.LogWarning("Client Had Differing Current Weather Value For ExtendedLevel: " + syncedWeather.Key.NumberlessPlanetName + ", Syncing!", DebugType.User);
                    syncedWeather.Key.SelectableLevel.currentWeather = syncedWeather.Value;
                }
            }
        }

        [ClientRpc]
        public void SetRandomExtendedDungeonFlowClientRpc(StringContainer[] dungeonFlowNames, int[] rarities)
        {
            DebugHelper.Log("Setting Random DungeonFlows!", DebugType.User);
            List<DungeonFlow> roundManagerFlows = Patches.RoundManager.GetDungeonFlows();
            IntWithRarity[] injectedDungeons = new IntWithRarity[dungeonFlowNames.Length];
            IntWithRarity[] cachedDungeons = LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes;
            Dictionary<string, int> dungeonFlowIds = new Dictionary<string, int>();

            for (int i = 0; i < roundManagerFlows.Count; i++)
                dungeonFlowIds.Add(roundManagerFlows[i].name, i);

            for (int i = 0; i < dungeonFlowNames.Length; i++)
                injectedDungeons[i] = Utilities.Create(dungeonFlowIds[dungeonFlowNames[i].SomeText], rarities[i]);

            LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes = injectedDungeons;
            Patches.RoundManager.GenerateNewFloor();
            LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes = cachedDungeons;
        }

        [ServerRpc]
        public void GetDungeonFlowSizeServerRpc()
        {
            SetDungeonFlowSizeClientRpc(DungeonLoader.GetClampedDungeonSize());
        }

        [ClientRpc]
        public void SetDungeonFlowSizeClientRpc(float hostSize)
        {
            Patches.RoundManager.dungeonGenerator.Generator.LengthMultiplier = hostSize;
            Patches.RoundManager.dungeonGenerator.Generate();
        }

        [ServerRpc]
        internal void SetExtendedLevelValuesServerRpc(ExtendedLevelData extendedLevelData)
        {
            if (PatchedContent.TryGetExtendedContent(extendedLevelData.UniqueIdentifier, out ExtendedLevel extendedLevel))
                SetExtendedLevelValuesClientRpc(extendedLevelData);
            else
                DebugHelper.Log("Failed To Send Level Info!", DebugType.User);
        }
        [ClientRpc]
        internal void SetExtendedLevelValuesClientRpc(ExtendedLevelData extendedLevelData)
        {
            if (PatchedContent.TryGetExtendedContent(extendedLevelData.UniqueIdentifier, out ExtendedLevel extendedLevel))
                extendedLevelData.ApplySavedValues(extendedLevel);
            else
                DebugHelper.Log("Failed To Apply Saved Level Info!", DebugType.User);
        }

        internal static void RegisterNetworkPrefab(ExtendedMod source, GameObject prefab)
        {
            if (prefab == null || source == null ) return;
            if (networkHasStarted == false)
                networkPrefabCollections.AddOrAddAdd(source, prefab);
            else
                DebugHelper.LogWarning("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!", DebugType.User);
        }

        [ServerRpc]
        public void TestContentReferenceServerRpc<E>(E content) where E : ExtendedContent
        {
            TestContentReferenceClientRpc
        }

        [ClientRpc]
        public void TestContentReferenceClientRpc()
        {

        }


        internal static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (prefab == null || queuedInternalNetworkPrefabs.Contains(prefab)) return;
            if (networkHasStarted == false)
                queuedInternalNetworkPrefabs.Add(prefab);
            else
                DebugHelper.LogWarning("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!", DebugType.User);
        }

        internal static void RegisterNetworkContent(ExtendedContent content)
        {
            List<GameObject> registeredObjects = new List<GameObject>();

            foreach (GameObject networkPrefab in content.GetNetworkPrefabsForRegistration())
                if (TryRegisterNetworkPrefab(networkPrefab))
                    registeredObjects.Add(networkPrefab);

            foreach (PrefabReference networkPrefabReference in content.GetPrefabReferencesForRestorationOrRegistration())
                if (TryRegisterNetworkPrefabReference(networkPrefabReference))
                    registeredObjects.Add(networkPrefabReference.Prefab);

            foreach (GameObject prefab in registeredObjects)
                networkPrefabCollections.AddOrAddAdd(content.ExtendedMod, prefab);
        }

        internal static void TrackVanillaPrefabs()
        {
            DebugHelper.Log(Events.FurthestState + "yyyyy", DebugType.User);
            if (Events.FurthestState == GameStates.Lobby || Events.FurthestState == GameStates.Moon) return;
            NetworkPrefabsList prefabList = Resources.FindObjectsOfTypeAll<NetworkPrefabsList>()[0]; //Need this because runs before singleton is set
            foreach (NetworkPrefab networkPrefab in prefabList.PrefabList)
            {
                DebugHelper.Log("Tracvking Vanilla1212", DebugType.User);
                AddNetworkPrefabToRegistry(networkPrefab);
                networkPrefabCollections.AddOrAddAdd(PatchedContent.VanillaMod, networkPrefab.Prefab);
                VanillaNetworkPrefabNameDict.Add(networkPrefab.Prefab.name, networkPrefab.Prefab);
            }
        }

        internal static void RegisterPrefabs()
        {
            //Register LethalLevelLoader's various Managers (Tracked via NetworkPrefabHandler Postfix)
            foreach (GameObject queuedPrefab in queuedInternalNetworkPrefabs)
                TryRegisterNetworkPrefab(queuedPrefab);

            //Register Each Mod's NetworkPrefabs (Tracked via NetworkPrefabHandler Postfix)
            //foreach (KeyValuePair<ExtendedMod, List<GameObject>> kvp in networkPrefabCollections)
                //foreach (GameObject prefab in kvp.Value)
                    //TryRegisterNetworkPrefab(prefab);

            networkHasStarted = true;      
        }

        private static bool TryRegisterNetworkPrefabReference(PrefabReference prefabReference)
        {
            if (VanillaNetworkPrefabNameDict.TryGetValue(prefabReference.Prefab.name, out GameObject vanillaPrefab))
            {
                prefabReference.Restore(vanillaPrefab);
                return (false);
            }
            else
                return (TryRegisterNetworkPrefab(prefabReference.Prefab));         
        }

        private static bool TryRegisterNetworkPrefab(GameObject gameObject)
        {
            if (gameObject == null) return (false);
            if (NetworkPrefabRegistry.ContainsKey(gameObject)) return (false);
            if (gameObject.TryGetComponent(out NetworkObject networkObject) == false) return (false);
            return (TryRegisterNetworkPrefab(networkObject));
        }

        private static bool TryRegisterNetworkPrefab(NetworkObject networkObject)
        {
            if (networkObject == null) return (false);
            if (NetworkPrefabRegistry.ContainsKey(networkObject.gameObject)) return (false);
            NetworkManagerInstance.AddNetworkPrefab(networkObject.gameObject);
            return (true);
        }

        //This gets called when we can access the NetworkManager for the first time and via a postfix to AddNetworkPrefab so we catch other mods too
        internal static void AddNetworkPrefabToRegistry(NetworkPrefab registeredPrefab)
        {
            if (NetworkPrefabRegistry.ContainsKey(registeredPrefab.Prefab)) return;
            NetworkPrefabRegistry.Add(registeredPrefab.Prefab, registeredPrefab);
        }


        public class StringContainer : INetworkSerializable
        {
            public string SomeText;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                if (serializer.IsWriter)
                    serializer.GetFastBufferWriter().WriteValueSafe(SomeText);
                else
                    serializer.GetFastBufferReader().ReadValueSafe(out SomeText);
            }
        }
    }
}
