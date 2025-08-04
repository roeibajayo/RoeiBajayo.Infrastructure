using System.Collections.Generic;

namespace Infrastructure.Utils.Http.Models;

public class RestCallOptions : RestClientOptions
{
    public IDictionary<string, object?>? Querystring { get; set; }
    public int Retries { get; set; }
    public int WaitMsBetweenRetries { get; set; }
}
