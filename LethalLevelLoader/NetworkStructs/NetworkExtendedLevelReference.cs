using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public struct NetworkExtendedLevelReference : INetworkSerializable
    {
        private List<ExtendedLevel> m_Levels => PatchedContent.ExtendedLevels;

        private uint m_ExtendedLevelId;
        private static uint s_NullId = uint.MaxValue;

        public uint ExtendedLevelId
        {
            get => m_ExtendedLevelId;
            internal set => m_ExtendedLevelId = value;
        }

        public NetworkExtendedLevelReference(ExtendedLevel level)
        {
            if (level == null)
            {
                m_ExtendedLevelId = s_NullId;
                return;
            }

            if (level.SelectableLevel == null)
            {
                throw new ArgumentException(level.name + "'s SelectableLevel is Missing!");
            }

            m_ExtendedLevelId = GetIndexIDFromExtendedLevel(level);
        }

        public bool TryGet(out ExtendedLevel level, NetworkManager networkManager = null)
        {
            level = Resolve(this);
            return (level != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ExtendedLevel Resolve(NetworkExtendedLevelReference level)
        {
            if (level.m_ExtendedLevelId == s_NullId)
                return null;
            return (level.GetExtendedLevelFromIndexID(level.ExtendedLevelId));
        }

        public static implicit operator ExtendedLevel(NetworkExtendedLevelReference levelRef) => Resolve(levelRef);

        public static implicit operator NetworkExtendedLevelReference(ExtendedLevel level) => new NetworkExtendedLevelReference(level);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_ExtendedLevelId);
        }

        private ExtendedLevel GetExtendedLevelFromIndexID(uint indexID)
        {
            for (int i = 0; i < m_Levels.Count; i++)
                if (m_Levels[i].SelectableLevel.levelID == indexID)
                        return (m_Levels[i]);
            return (null);
        }

        private uint GetIndexIDFromExtendedLevel(ExtendedLevel level) => (uint)level.SelectableLevel.levelID;
    }
}
