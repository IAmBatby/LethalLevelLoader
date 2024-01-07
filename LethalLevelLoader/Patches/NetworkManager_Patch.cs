using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class NetworkManager_Patch
    {
        private static List<GameObject> queuedNetworkPrefabs = new List<GameObject>();
        public static bool networkHasStarted;

        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (networkHasStarted == false)
                queuedNetworkPrefabs.Add(prefab);
            else
                DebugHelper.Log("Attempted To Register NetworkPrefab: " + prefab + " After GameNetworkManager Has Started!");
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(0)]
        private static void GameNetworkManager_Start(GameNetworkManager __instance)
        {
            DebugHelper.Log("Game NetworkManager Start");

            Unity.Netcode.NetworkManager networkManager = __instance.GetComponent<Unity.Netcode.NetworkManager>();

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
