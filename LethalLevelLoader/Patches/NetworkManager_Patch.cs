using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace LethalLevelLoader
{
    public class NetworkManager_Patch
    {
        private static List<GameObject> queuedNetworkPrefabs = new List<GameObject>();

        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            queuedNetworkPrefabs.Add(prefab);
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        private static void GameNetworkManager_Start(GameNetworkManager __instance)
        {
            DebugHelper.Log("Game NetworkManager Start");

            Unity.Netcode.NetworkManager networkManager = __instance.GetComponent<Unity.Netcode.NetworkManager>();

            foreach (GameObject queuedNetworkPrefab in queuedNetworkPrefabs)
            {
                networkManager.AddNetworkPrefab(queuedNetworkPrefab);
            }
            
        }
    }
}
