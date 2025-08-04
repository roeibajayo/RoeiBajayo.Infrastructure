using System.Text.Json;

namespace Infrastructure.Utils.Http.Models;

public class JsonBodyRestCallOptions : BodyRestCallOptions
{
    public required object Json { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
}
