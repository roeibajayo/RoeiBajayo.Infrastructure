namespace Infrastructure.Utils.Repositires.Persistent
{
    public class MessagePackPersistentCollectionConnection<T> : PersistentCollectionConnection<T> where T : class
    {
        public MessagePackPersistentCollectionConnection(string basePath, bool useIndexFile = true) :
            this(basePath, typeof(T).Name, useIndexFile, 1000)
        { }
        public MessagePackPersistentCollectionConnection(string basePath, string name, bool useIndexFile = true) :
            this(basePath, name, useIndexFile, 1000)
        { }
        public MessagePackPersistentCollectionConnection(string basePath, string name, bool useIndexFile = true, int intervalMiliseconds = 1000) :
            base(basePath, name, useIndexFile, intervalMiliseconds)
        { }

        protected override PersistentCollection<T> CreateCollectionInstance()
        {
            return new MessagePackPersistentCollection<T>(_basePath, _name, _useIndexFile, _intervalMiliseconds);
        }
    }
}
