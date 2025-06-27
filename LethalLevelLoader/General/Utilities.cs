using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ExtendedNetworkManager.RegisterNetworkPrefab(go);
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
            ExtendedNetworkManager.RegisterNetworkPrefab(go);
            return (component as T);
        }

        public static IntWithRarity Create(int id, int rarity)
        {
            IntWithRarity returnR = new IntWithRarity();
            returnR.id = id;
            returnR.rarity = rarity;
            return (returnR);
        }

        public static IndoorMapType Create(DungeonFlow dungeonFlow, float mapTileSize, AudioClip firstTimeAudio)
        {
            IndoorMapType returnR = new IndoorMapType();
            returnR.dungeonFlow = dungeonFlow;
            returnR.MapTileSize = mapTileSize;
            returnR.firstTimeAudio = firstTimeAudio;
            return (returnR);
        }

        public static SpawnableEnemyWithRarity Create(EnemyType enemy, int rarity)
        {
            SpawnableEnemyWithRarity returnR = new SpawnableEnemyWithRarity();
            returnR.enemyType = enemy;
            returnR.rarity = rarity;
            return (returnR);
        }

        public static CompatibleNoun Create(TerminalKeyword noun, TerminalNode result)
        {
            CompatibleNoun returnR = new CompatibleNoun();
            returnR.result = result;
            returnR.noun = noun;
            return (returnR);
        }

        internal static void Insert<T>(ref T[] array, T newItem)
        {
            array = array.AddItem(newItem).ToArray();
        }
    }
}
