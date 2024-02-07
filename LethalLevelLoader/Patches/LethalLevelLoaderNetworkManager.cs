using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using JetBrains.Annotations;
using DunGen.Graph;
using System.Linq;

namespace LethalLevelLoader
{
    public class LethalLevelLoaderNetworkManager : NetworkBehaviour
    {
        public static GameObject networkingManagerPrefab;
        private static LethalLevelLoaderNetworkManager _instance;
        public static LethalLevelLoaderNetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = UnityEngine.Object.FindObjectOfType<LethalLevelLoaderNetworkManager>();
                if (_instance == null)
                    DebugHelper.LogWarning("LethalLevelLoaderNetworkManager Could Not Be Found! Returning Null!");
                return _instance;
            }
            set { _instance = value; }
        }

        private static List<GameObject> queuedNetworkPrefabs = new List<GameObject>();
        public static bool networkHasStarted;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = "LethalLevelLoaderNetworkManager";
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [ServerRpc]
        public void GetRandomExtendedDungeonFlowServerRpc()
        {
            DebugHelper.Log("Getting Random DungeonFlow!");

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonManager.GetValidExtendedDungeonFlows(LevelManager.CurrentExtendedLevel, debugResults: true);

            List<int> dungeonFlowIDs = new List<int>();
            List<int> rarities = new List<int>();

            if (availableExtendedFlowsList.Count == 0)
            {
                DebugHelper.LogError("No ExtendedDungeonFlow's could be found! This should only happen if the Host's requireMatchesOnAllDungeonFlows is set to true!");
                DebugHelper.LogError("Loading Facility DungeonFlow to prevent infinite loading!");
                dungeonFlowIDs.Add(0);
                rarities.Add(300);
            }
            else
            {
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in availableExtendedFlowsList)
                {
                    dungeonFlowIDs.Add(RoundManager.Instance.dungeonFlowTypes.ToList().IndexOf(extendedDungeonFlowWithRarity.extendedDungeonFlow.dungeonFlow));
                    rarities.Add(extendedDungeonFlowWithRarity.rarity);
                }
            }

            SetRandomExtendedDungeonFlowClientRpc(dungeonFlowIDs.ToArray(), rarities.ToArray());
        }

        [ClientRpc]
        public void SetRandomExtendedDungeonFlowClientRpc(int[] dungeonFlowIDs, int[] rarities)
        {
            DebugHelper.Log("Setting Random DungeonFlow!");
            List<IntWithRarity> dungeonFlowsList = new List<IntWithRarity>();
            List<IntWithRarity> cachedDungeonFlowsList = new List<IntWithRarity>();

            for (int i = 0; i < dungeonFlowIDs.Length; i++)
            {
                IntWithRarity intWithRarity = new IntWithRarity();
                intWithRarity.Add(dungeonFlowIDs[i], rarities[i]);
                dungeonFlowsList.Add(intWithRarity);
            }
            cachedDungeonFlowsList = new List<IntWithRarity>(LevelManager.CurrentExtendedLevel.selectableLevel.dungeonFlowTypes.ToList());
            LevelManager.CurrentExtendedLevel.selectableLevel.dungeonFlowTypes = dungeonFlowsList.ToArray();
            RoundManager.Instance.GenerateNewFloor();
            LevelManager.CurrentExtendedLevel.selectableLevel.dungeonFlowTypes = cachedDungeonFlowsList.ToArray();
        }

        [ServerRpc]
        public void GetDungeonFlowSizeServerRpc()
        {
            SetDungeonFlowSizeClientRpc(DungeonLoader.GetClampedDungeonSize());
        }

        [ClientRpc]
        public void SetDungeonFlowSizeClientRpc(float hostSize)
        {
            RoundManager.Instance.dungeonGenerator.Generator.LengthMultiplier = hostSize;
            RoundManager.Instance.dungeonGenerator.Generate();
        }


        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (networkHasStarted == false)
                queuedNetworkPrefabs.Add(prefab);
            else
                DebugHelper.Log("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!");
        }

        internal static void RegisterPrefabs(NetworkManager networkManager)
        {
            //DebugHelper.Log("Game NetworkManager Start");

            List<GameObject> addedNetworkPrefabs = new List<GameObject>();

            foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
                addedNetworkPrefabs.Add(networkPrefab.Prefab);

            int debugCounter = 0;

            foreach (GameObject queuedNetworkPrefab in queuedNetworkPrefabs)
            {
                if (!addedNetworkPrefabs.Contains(queuedNetworkPrefab))
                {
                    //DebugHelper.Log("Trying To Register Prefab: " + queuedNetworkPrefab);
                    networkManager.AddNetworkPrefab(queuedNetworkPrefab);
                    addedNetworkPrefabs.Add(queuedNetworkPrefab);
                }
                else
                    debugCounter++;
            }

            DebugHelper.Log("Skipped Registering " + debugCounter + " NetworkObjects As They Were Already Registered.");

            networkHasStarted = true;
            
        }
    }
}
