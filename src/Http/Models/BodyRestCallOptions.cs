namespace Infrastructure.Utils.Http.Models;

public abstract class BodyRestCallOptions : RestCallOptions
{
    public string? ContentType { get; set; }
}
