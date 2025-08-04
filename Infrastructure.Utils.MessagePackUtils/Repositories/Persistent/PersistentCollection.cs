using MessagePack;
using System;
using System.IO;

namespace Infrastructure.Utils.Repositires.Persistent
{
    public class MessagePackPersistentCollection<T> : PersistentCollection<T> where T : class
    {
        public MessagePackPersistentCollection(bool useIndexFile, int intervalMiliseconds = 1000) :
            base(useIndexFile, intervalMiliseconds)
        { }

        public MessagePackPersistentCollection(string basePath, bool useIndexFile, int intervalMiliseconds = 1000) :
            this(basePath, typeof(T).Name, useIndexFile, intervalMiliseconds)
        { }

        public MessagePackPersistentCollection(string basePath, string name, bool useIndexFile, int intervalMiliseconds = 1000) :
            base(basePath, name, useIndexFile, intervalMiliseconds)
        {
        }

        protected override TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes) =>
            MessagePackSerializer.Deserialize<TType>(bytes.ToArray());

        protected override TType DeserializeDocument<TType>(Stream stream) =>
            MessagePackSerializer.Deserialize<TType>(stream);

        protected override byte[] SerializeDocument<TType>(TType item) =>
            MessagePackSerializer.Serialize(item);

    }
}
