using MessagePack;
using System;

namespace Infrastructure.Utils.Repositires.Database
{
    public class MessagePackInfileDatabase<T> : InfileDatabase<T>
    {
        public MessagePackInfileDatabase(string basePath, string name, string idProperty = null) :
            base(basePath, name, idProperty)
        {
        }

        protected override TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes) =>
            MessagePackSerializer.Deserialize<TType>(bytes.ToArray());

        protected override byte[] SerializeDocument<TType>(TType item) =>
            MessagePackSerializer.Serialize(item);
    }
}
