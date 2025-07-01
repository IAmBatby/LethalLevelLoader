using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public abstract class NetworkSingleton : NetworkBehaviour
    {
        public static NetworkManager NetworkManagerInstance => NetworkManager.Singleton;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(NetworkSingleton))) continue;
                if (Plugin.SetupObject.AddComponent(type) is NetworkSingleton manager)
                    manager.CreateNetworkPrefab();
            }
        }

        protected abstract void CreateNetworkPrefab();
    }


    public abstract class NetworkSingleton<T> : NetworkSingleton where T : NetworkSingleton<T>
    {
        public static NetworkSingleton<T> NetworkPrefab { get; private set; }
        public static NetworkSingleton<T> NetworkInstance { get; private set; }

        private static List<Action> queuedInitializedFunctions = new List<Action>();

        public static bool IsSpawnedAndIntialized => (NetworkInstance != null && NetworkInstance.IsSpawned && Events.InInitializedLobby);

        public sealed override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkInstance = this;
            ExtendedNetworkManager.OnNetworkSingletonSpawned(this);
            OnNetworkSingletonSpawn();
            StartCoroutine(InvokeSpawnedInitialize());
        }

        private IEnumerator InvokeSpawnedInitialize()
        {
            yield return IsSpawnedAndIntialized ? null : new WaitUntil(() => IsSpawnedAndIntialized);
            DebugHelper.Log("Spawned Initializing: " + this.GetType(), DebugType.User);
            OnNetworkSingletonSpawnedInitialize();
            foreach (Action action in queuedInitializedFunctions)
                action();
            queuedInitializedFunctions.Clear();
        }

        public sealed override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            NetworkInstance = null;
            //ExtendedNetworkManager.OnNetworkSingletonDespawn(this);
            OnNetworkSingletonDespawn();
        }

        protected virtual void OnNetworkSingletonSpawn() { }
        protected virtual void OnNetworkSingletonSpawnedInitialize() { }
        protected virtual void OnNetworkSingletonDespawn() { }

        public static void InvokeWhenInitalized(Action act)
        {
            if (IsSpawnedAndIntialized)
                act();
            else
                queuedInitializedFunctions.Add(act);
        }

        protected override sealed void CreateNetworkPrefab()
        {
            NetworkPrefab = ExtendedNetworkManager.CreateAndRegisterNetworkSingleton<NetworkSingleton<T>>(GetType(), typeof(T).Name + " (NetworkPrefab)");
            GameObject.Destroy(this);
        }
    }
}
