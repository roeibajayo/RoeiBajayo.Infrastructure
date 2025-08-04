namespace Infrastructure.Utils.Repositories.Database;

public class JsonInfileConnection<T> : InfileConnection<T>
{
    public JsonInfileConnection() : base()
    {
    }

    public JsonInfileConnection(string basePath) : base(basePath)
    {
    }

    public JsonInfileConnection(string basePath, string name) : base(basePath, name)
    {
    }

    protected override InfileDatabase<T> CreateCollectionInstance() =>
        new JsonInfileDatabase<T>(_basePath, _name);
}
