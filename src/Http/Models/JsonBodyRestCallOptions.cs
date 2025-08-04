using System.Text.Json;

namespace RoeiBajayo.Infrastructure.Http.Models;

public class JsonBodyRestCallOptions : BodyRestCallOptions
{
    public required object Json { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
}
