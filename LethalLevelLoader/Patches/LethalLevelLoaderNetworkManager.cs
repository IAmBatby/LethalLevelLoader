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
            DebugHelper.Log("Networking Manager Spawned!");
            //gameObject.name = "LethalLevelLoaderNetworkManager";
            Instance = this;
            DontDestroyOnLoad(gameObject);
            TestRpcs();
        }

        public void TestRpcs()
        {
            if (IsServer)
                TestLogServerRpc();
        }

        [ServerRpc]
        public void TestLogServerRpc()
        {
            DebugHelper.Log("This is a Server Rpc Call!");
            TestLogClientRpc();
        }

        [ClientRpc]
        public void TestLogClientRpc()
        {
            DebugHelper.Log("This is a Client Rpc Call!");
        }

        [ServerRpc]
        public void GetRandomExtendedDungeonFlowServerRpc()
        {
            DebugHelper.Log("Getting Random DungeonFlow!");

            List<int> randomWeightsList = new List<int>();

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonManager.GetValidExtendedDungeonFlows(LevelManager.CurrentExtendedLevel, debugResults: true);

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
                randomWeightsList.Add(extendedDungeon.rarity);

            ExtendedDungeonFlow extendedDungeonFlow = availableExtendedFlowsList[RoundManager.Instance.GetRandomWeightedIndex(randomWeightsList.ToArray(), RoundManager.Instance.LevelRandom)].extendedDungeonFlow;
            //dungeonGenerator.DungeonFlow = extendedDungeonFlow.dungeonFlow;

            string debugString = "Current Level + (" + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + ") Weights List: " + "\n" + "\n";

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
            {
                if (extendedDungeon.extendedDungeonFlow == extendedDungeonFlow)
                    debugString += extendedDungeon.extendedDungeonFlow.dungeonFlow.name + " | " + extendedDungeon.rarity + " - Selected DungeonFlow" + "\n";
                else
                    debugString += extendedDungeon.extendedDungeonFlow.dungeonFlow.name + " | " + extendedDungeon.rarity + "\n";
            }

            DebugHelper.Log(debugString + "\n");

            SetRandomExtendedDungeonFlowClientRpc(RoundManager.Instance.dungeonFlowTypes.ToList().IndexOf(extendedDungeonFlow.dungeonFlow));
        }

        [ClientRpc]
        public void SetRandomExtendedDungeonFlowClientRpc(int dungeonFlowIndex)
        {
            DebugHelper.Log("Setting Random DungeonFlow!");
            DebugHelper.Log("DungeonFlow Recieved Was: " + RoundManager.Instance.dungeonFlowTypes[dungeonFlowIndex].name);
            RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow = RoundManager.Instance.dungeonFlowTypes[dungeonFlowIndex];
            DungeonLoader.PrepareDungeon();
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
                    DebugHelper.Log("Trying To Register Prefab: " + queuedNetworkPrefab);
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
