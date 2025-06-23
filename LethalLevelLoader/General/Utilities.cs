using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public static class Utilities
    {
        public static T CreateNetworkPrefab<T>(string name, bool dontDestroyWithOwner = false, bool sceneMigration = true, bool destroyWithScene = true) where T : NetworkBehaviour
        {
            GameObject go = PrefabHelper.CreateNetworkPrefab(name);
            T component = go.AddComponent<T>();
            NetworkObject ngo = go.GetComponent<NetworkObject>();
            ngo.DontDestroyWithOwner = dontDestroyWithOwner;
            ngo.SceneMigrationSynchronization = sceneMigration;
            ngo.DestroyWithScene = destroyWithScene;
            LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(go);
            return (component);
        }

        internal static T CreateNetworkPrefab<T>(Type realType, string name, bool dontDestroyWithOwner = false, bool sceneMigration = true, bool destroyWithScene = true) where T : NetworkBehaviour
        {
            GameObject go = PrefabHelper.CreateNetworkPrefab(name);
            var component = go.AddComponent(realType);
            NetworkObject ngo = go.GetComponent<NetworkObject>();
            ngo.DontDestroyWithOwner = dontDestroyWithOwner;
            ngo.SceneMigrationSynchronization = sceneMigration;
            ngo.DestroyWithScene = destroyWithScene;
            LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(go);
            return (component as T);
        }
    }
}
