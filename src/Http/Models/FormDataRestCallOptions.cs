using System.Collections.Generic;

namespace Infrastructure.Utils.Http.Models;

public class FormDataRestCallOptions : RestCallOptions
{
    public required IDictionary<string, object> FormData { get; set; }
}
