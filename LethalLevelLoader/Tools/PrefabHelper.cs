using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public class PrefabHelper
    {
        internal static Lazy<GameObject> _prefabParent;
        internal static GameObject prefabParent { get { return _prefabParent.Value; } }

        static PrefabHelper()
        {
            _prefabParent = new Lazy<GameObject>(() =>
            {
                var parent = new GameObject("LethalLibGeneratedPrefabs");
                parent.hideFlags = HideFlags.HideAndDontSave;
                parent.SetActive(false);

                return parent;
            });
        }

        public static GameObject CreatePrefab(string name)
        {
            var prefab = new GameObject(name);
            prefab.hideFlags = HideFlags.HideAndDontSave;

            prefab.transform.SetParent(prefabParent.transform);

            return prefab;
        }

        public static GameObject CreateNetworkPrefab(string name)
        {
            var prefab = CreatePrefab(name);
            prefab.AddComponent<NetworkObject>();

            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + name));

            prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);

            //LethalLevelLoaderNetworkManager.RegisterNetworkPrefab(prefab);
            return prefab;
        }
    }
}
