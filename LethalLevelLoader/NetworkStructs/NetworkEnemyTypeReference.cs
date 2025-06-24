using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public struct NetworkEnemyTypeReference : INetworkSerializable
    {
        private List<NetworkPrefab> m_Prefabs => ExtendedNetworkManager.NetworkManagerInstance.NetworkConfig.Prefabs.m_Prefabs;

        private uint m_NetworkEnemyTypeObjectId;
        private static uint s_NullId = uint.MaxValue;

        public uint NetworkEnemyTypeObjectId
        {
            get => m_NetworkEnemyTypeObjectId;
            internal set => m_NetworkEnemyTypeObjectId = value;
        }

        public NetworkEnemyTypeReference(EnemyType enemy)
        {
            if (enemy == null)
            {
                m_NetworkEnemyTypeObjectId = s_NullId;
                return;
            }

            if (enemy.enemyPrefab == null || enemy.enemyPrefab.GetComponent<EnemyAI>() == false)
            {
                throw new ArgumentException(enemy.name + "'s Prefab or Prefab GrabbableObject is Missing!");
            }

            m_NetworkEnemyTypeObjectId = GetIdHashFromEnemyType(enemy);
        }

        public bool TryGet(out EnemyType enemy, NetworkManager networkManager = null)
        {
            enemy = Resolve(this);
            return (enemy != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EnemyType Resolve(NetworkEnemyTypeReference networkEnemy)
        {
            if (networkEnemy.m_NetworkEnemyTypeObjectId == s_NullId)
                return null;
            return (networkEnemy.GetEnemyTypeFromNetworkPrefabIdHash(networkEnemy.m_NetworkEnemyTypeObjectId));
        }

        public static implicit operator EnemyType(NetworkEnemyTypeReference networkEnemyRef) => Resolve(networkEnemyRef);

        public static implicit operator NetworkEnemyTypeReference(EnemyType enemy) => new NetworkEnemyTypeReference(enemy);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_NetworkEnemyTypeObjectId);
        }

        private EnemyType GetEnemyTypeFromNetworkPrefabIdHash(uint idHash)
        {
            for (int i = 0; i < m_Prefabs.Count; i++)
                if (m_Prefabs[i].SourcePrefabGlobalObjectIdHash == idHash)
                    if (m_Prefabs[i].Prefab.TryGetComponent(out EnemyAI enemyAI))
                        return (enemyAI.enemyType);
            return (null);
        }

        private uint GetIdHashFromEnemyType(EnemyType enemy)
        {
            for (int i = 0; i < m_Prefabs.Count; i++)
                if (m_Prefabs[i].Prefab == enemy.enemyPrefab)
                    return (m_Prefabs[i].SourcePrefabGlobalObjectIdHash);
            return (0);
        }
    }
}
