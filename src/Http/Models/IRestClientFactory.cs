namespace RoeiBajayo.Infrastructure.Http.Models;

public interface IRestClientFactory
{
    IRestClient GetClient(string? name = null);
    void ResetClient(string? name = null);
}