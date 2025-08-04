using System.IO;

namespace RoeiBajayo.Infrastructure.Http.Models;

public class StreamBodyRestCallOptions : BodyRestCallOptions
{
    public required Stream Body { get; set; }
}
