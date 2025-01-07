using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader.NetworkStructs
{
    public struct NetworkItemReference : INetworkSerializable
    {
        private uint m_NetworkItemObjectId;
        private static uint s_NullId = uint.MaxValue;

        public uint NetworkItemObjectId
        {
            get => m_NetworkItemObjectId;
            internal set => m_NetworkItemObjectId = value;
        }

        public NetworkItemReference(Item item)
        {
            if (item == null)
            {
                m_NetworkItemObjectId = s_NullId;
                return;
            }

            if (item.spawnPrefab == null || item.spawnPrefab.GetComponent<GrabbableObject>() == false)
            {
                throw new ArgumentException(item.name + "'s Prefab or Prefab GrabbableObject is Missing!");
            }

            m_NetworkItemObjectId = GetIdHashFromItem(item);
        }

        public bool TryGet(out Item item, NetworkManager networkManager = null)
        {
            item = Resolve(this);
            return (item != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Item Resolve(NetworkItemReference networkItemRef)
        {
            if (networkItemRef.m_NetworkItemObjectId == s_NullId)
                return null;
            return (networkItemRef.GetItemFromNetworkPrefabIdHash(networkItemRef.m_NetworkItemObjectId));
        }

        public static implicit operator Item(NetworkItemReference networkItemRef) => Resolve(networkItemRef);

        public static implicit operator NetworkItemReference(Item item) => new NetworkItemReference(item);

        private List<NetworkPrefab> m_Prefabs => LethalLevelLoaderNetworkManager.networkManager.NetworkConfig.Prefabs.m_Prefabs;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_NetworkItemObjectId);
        }

        private Item GetItemFromNetworkPrefabIdHash(uint idHash)
        {
            for (int i = 0; i < m_Prefabs.Count; i++)
                if (m_Prefabs[i].SourcePrefabGlobalObjectIdHash == idHash)
                    if (m_Prefabs[i].Prefab.TryGetComponent(out GrabbableObject grabbableObject))
                        return (grabbableObject.itemProperties);
            return (null);
        }

        private uint GetIdHashFromItem(Item item)
        {
            for (int i = 0; i < m_Prefabs.Count; i++)
                if (m_Prefabs[i].Prefab == item.spawnPrefab)
                    return (m_Prefabs[i].SourcePrefabGlobalObjectIdHash);
            return (0);
        }
    }
}
