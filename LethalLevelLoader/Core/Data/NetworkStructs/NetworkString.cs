using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class NetworkString : INetworkSerializable
    {
        public string StringValue;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
                serializer.GetFastBufferWriter().WriteValueSafe(StringValue);
            else
                serializer.GetFastBufferReader().ReadValueSafe(out StringValue);
        }
    }
}
