namespace Infrastructure.Utils.DependencyInjection.Interfaces;

/// <summary>
/// Represents a service with a singleton lifetime.
/// </summary>
/// <remarks>Make sure to use <see cref="AddDependencyInjectionServices{TMarker}()"/> to register all services that inherit from this interface.</remarks>
public interface ISingletonService;

/// <summary>
/// Represents a service with a singleton lifetime that operates on a specific service type.
/// </summary>
/// <typeparam name="T">The type of object that the service operates on.</typeparam>
/// <remarks>Make sure to use <see cref="AddDependencyInjectionServices{TMarker}()"/> to register all services that inherit from this interface.</remarks>
public interface ISingletonService<T>;
