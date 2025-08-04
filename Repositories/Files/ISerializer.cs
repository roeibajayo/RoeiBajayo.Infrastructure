namespace Infrastructure.Utils.Repositories.Files;

public interface ISerializer<T>
{
    string? Serialize(T content);
    T? Deserialize(string? content);
}
