using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Http.Models;

public class MultipartFormDataRestCallOptions : RestCallOptions
{
    public required IDictionary<string, MultipartFormDataValue> FormData { get; set; }
    public bool LeaveOpen { get; set; }
}
