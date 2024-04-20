using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using JetBrains.Annotations;
using DunGen.Graph;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Reflection;

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
            DebugHelper.Log("Getting Random DungeonFlows!");

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonManager.GetValidExtendedDungeonFlows(LevelManager.CurrentExtendedLevel, debugResults: true);

            //List<string> dungeonFlowNames = new List<string>();
            List<StringContainer> dungeonFlowNames = new List<StringContainer>();
            List<int> rarities = new List<int>();

            if (availableExtendedFlowsList.Count == 0)
            {
                DebugHelper.LogError("No ExtendedDungeonFlow's could be found! This should only happen if the Host's requireMatchesOnAllDungeonFlows is set to true!");
                DebugHelper.LogError("Loading Facility DungeonFlow to prevent infinite loading!");
                StringContainer newStringContainer = new StringContainer();
                newStringContainer.SomeText = PatchedContent.ExtendedDungeonFlows[0].dungeonFlow.name;
                dungeonFlowNames.Add(newStringContainer);
                rarities.Add(300);
            }
            else
            {
                List<DungeonFlow> dungeonFlowTypes = Patches.RoundManager.GetDungeonFlows();
                foreach (ExtendedDungeonFlowWithRarity extendedDungeonFlowWithRarity in availableExtendedFlowsList)
                {
                    StringContainer newStringContainer = new StringContainer();
                    newStringContainer.SomeText = dungeonFlowTypes[dungeonFlowTypes.IndexOf(extendedDungeonFlowWithRarity.extendedDungeonFlow.dungeonFlow)].name;
                    dungeonFlowNames.Add(newStringContainer);

                    rarities.Add(extendedDungeonFlowWithRarity.rarity);
                }
            }

            SetRandomExtendedDungeonFlowClientRpc(dungeonFlowNames.ToArray(), rarities.ToArray());
        }

        [ClientRpc]
        public void SetRandomExtendedDungeonFlowClientRpc(StringContainer[] dungeonFlowNames, int[] rarities)
        {
            DebugHelper.Log("Setting Random DungeonFlows!");
            List<IntWithRarity> dungeonFlowsList = new List<IntWithRarity>();
            List<IntWithRarity> cachedDungeonFlowsList = new List<IntWithRarity>();
            
            Dictionary<string, int> dungeonFlowIds = new Dictionary<string, int>();
            int counter = 0;
            foreach (DungeonFlow dungeonFlow in Patches.RoundManager.GetDungeonFlows())
            {
                dungeonFlowIds.Add(dungeonFlow.name, counter);
                counter++;
            }    
            for (int i = 0; i < dungeonFlowNames.Length; i++)
            {
                IntWithRarity intWithRarity = new IntWithRarity();
                intWithRarity.Add(dungeonFlowIds[dungeonFlowNames[i].SomeText], rarities[i]);
                dungeonFlowsList.Add(intWithRarity);
            }
            cachedDungeonFlowsList = new List<IntWithRarity>(LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes.ToList());
            LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes = dungeonFlowsList.ToArray();
            Patches.RoundManager.GenerateNewFloor();
            LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes = cachedDungeonFlowsList.ToArray();
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

        public class StringContainer : INetworkSerializable
        {
            public string SomeText;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                if (serializer.IsWriter)
                {
                    serializer.GetFastBufferWriter().WriteValueSafe(SomeText);
                }
                else
                {
                    serializer.GetFastBufferReader().ReadValueSafe(out SomeText);
                }
            }
        }
    }
}
