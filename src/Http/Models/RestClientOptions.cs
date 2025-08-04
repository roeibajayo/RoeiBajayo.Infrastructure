using Infrastructure.Utils.Repositories.Queues.Throttling.Models;
using System;
using System.Collections.Generic;

namespace Infrastructure.Utils.Http.Models;

public class RestClientOptions
{
    public string? BaseUrl { get; set; }
    public bool? AutoRedirect { get; set; }
    public bool? IgnoreBadCertificates { get; set; }
    public IDictionary<string, string>? Headers { get; set; }
    public bool? UseCookies { get; set; }
    public ICookiesRepository? Cookies { get; set; }
    public bool? FakeUserAgent { get; set; }
    public bool? DefaultHeaders { get; set; }
    public string? Proxy { get; set; }
    public bool? UseOnlineProxy { get; set; }
    public bool? SkipEnsureSuccessStatusCode { get; set; }
    public ThrottlingTimeSpan[]? Throttling { get; set; }
    public TimeSpan? Timeout { get; set; }
}
