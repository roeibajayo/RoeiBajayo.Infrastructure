using System;
using System.Net.Http;

namespace Infrastructure.Utils.Http.Models;

public sealed class InvalidResponseException(string message, Exception innerException) : 
    Exception(message, innerException)
{
    public required HttpRequestMessage Request { get; init; }
    public HttpResponseMessage? Response { get; init; }
    public string? Content { get; init; }
}
