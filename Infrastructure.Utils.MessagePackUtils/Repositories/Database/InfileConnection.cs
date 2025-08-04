namespace Infrastructure.Utils.Repositires.Database
{
    public class MessagePackInfileConnection<T> : InfileConnection<T>
    {
        public MessagePackInfileConnection() : base()
        {
        }

        public MessagePackInfileConnection(string basePath) : base(basePath)
        {
        }

        public MessagePackInfileConnection(string basePath, string name) : base(basePath, name)
        {
        }

        protected override InfileDatabase<T> CreateCollectionInstance() =>
            new MessagePackInfileDatabase<T>(_basePath, _name);
    }
}
