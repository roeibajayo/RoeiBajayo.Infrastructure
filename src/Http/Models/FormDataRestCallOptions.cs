using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Http.Models;

public class FormDataRestCallOptions : RestCallOptions
{
    public required IDictionary<string, object> FormData { get; set; }
}
