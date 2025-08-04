using RoeiBajayo.Infrastructure.DependencyInjection;
using RoeiBajayo.Infrastructure.DependencyInjection.ScopedCache;
using RoeiBajayo.Infrastructure.Http.Models;
using MemoryCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace RoeiBajayo.Infrastructure;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// This method adds the infrastructure services to the service collection and register all interface-based DI component by the provided marker type.
    /// Services added: register all Mediator services, Memory cache component, Scoped cache component, Lazy DI support, and default RestClientOptions.
    /// </summary>
    /// <typeparam name="TMaker">Marker type for identifying the required Assembly.</typeparam>
    public static void AddInfrastructureServices<TMaker>(this IServiceCollection services)
    {
        var infrastructureMarker = typeof(IInfrastructureMarker);
        if (!services.Any(x => x.ServiceType == infrastructureMarker))
            services.AddInfrastructureServices();

        services.AddMediatorCore<TMaker>();
        services.AddDependencyInjectionServices<TMaker>();
    }

    /// <summary>
    /// This method adds the infrastructure services to the service collection.
    /// Services added: register all Mediator services, Memory cache component, Scoped cache component, Lazy DI support, and default RestClientOptions.
    /// </summary>
    private static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IInfrastructureMarker>(x => null!);
        services.AddMediatorCore<IInfrastructureMarker>();
        services.AddMemoryCore();
        services.AddScopedCached();
        services.AddLazySupport();
        services.AddSingleton((services) => new RestClientOptions
        {
            FakeUserAgent = true,
        });
        services.AddDependencyInjectionServices<IInfrastructureMarker>();
    }
}
