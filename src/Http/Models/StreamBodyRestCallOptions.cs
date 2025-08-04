using System.IO;

namespace Infrastructure.Utils.Http.Models;

public class StreamBodyRestCallOptions : BodyRestCallOptions
{
    public required Stream Body { get; set; }
}
