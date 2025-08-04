using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Infrastructure.Utils.Http.Models;

public class ProblemJsonException(int statusCode, string type, string title, string detail, string instance, 
    Dictionary<string, JsonNode?> extensions) : Exception
{
    public int StatusCode { get; set; } = statusCode;
    public string Type { get; set; } = type;
    public string Title { get; set; } = title;
    public string Detail { get; set; } = detail;
    public string Instance { get; set; } = instance;
    public Dictionary<string, JsonNode?> Extensions { get; } = extensions;
}