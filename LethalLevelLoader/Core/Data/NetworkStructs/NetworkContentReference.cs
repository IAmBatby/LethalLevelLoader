using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public struct NetworkContentReference<E> : INetworkSerializable where E : ExtendedContent, IManagedContent, IExtendedContent
    {
        private uint m_NetworkContentIndexId;
        private static uint s_NullId = uint.MaxValue;
        public bool IsInvalid => NetworkContentIndexId == s_NullId;

        public uint NetworkContentIndexId { get => m_NetworkContentIndexId; internal set => m_NetworkContentIndexId = value; }

        public NetworkContentReference(E extendedContent)
        {
            m_NetworkContentIndexId = extendedContent == null ? s_NullId : (uint)ExtendedContentManager<E>.ExtendedContents.IndexOf(extendedContent);
        }

        public bool TryGetComponent(out E extendedContent, NetworkManager networkManager = null)
        {
            extendedContent = Resolve(this);
            return (extendedContent != null);
        }

        public E GetContent() => Resolve(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static E Resolve(NetworkContentReference<E> networkContent)
        {
            return (networkContent.IsInvalid ? null : ExtendedContentManager<E>.ExtendedContents[(int)networkContent.NetworkContentIndexId]);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_NetworkContentIndexId);
        }

        public static implicit operator E(NetworkContentReference<E> networkContentRef) => Resolve(networkContentRef);
        public static implicit operator NetworkContentReference<E>(E extendedContent) => new NetworkContentReference<E>(extendedContent);
    }
}
