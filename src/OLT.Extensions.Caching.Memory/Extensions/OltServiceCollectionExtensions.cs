using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Core;

public static class OltMemoryCacheServiceCollectionExtensions
{
    /// <summary>
    /// Adds Memory Cache 
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IMemoryCache"/> and <see cref="IOltMemoryCache"/> service.
    /// </remarks>
    /// <param name="services"><seealso cref="IServiceCollection"/></param>
    /// <param name="defaultAbsoluteExpiration">Default expire cache at. (uses default if not supplied)</param>
    /// <returns><seealso cref="IServiceCollection"/></returns>
    [Obsolete("Removing in 10.x, Use AddOltHybridCache")]
    public static IServiceCollection AddOltCacheMemory(this IServiceCollection services, TimeSpan defaultAbsoluteExpiration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IOltMemoryCache, OltMemoryCache>();
        services.TryAddSingleton<IOltCacheService>((sp) => sp.GetRequiredService<IOltMemoryCache>());
        return services.AddMemoryCache(opt => new MemoryCacheEntryOptions().SetAbsoluteExpiration(defaultAbsoluteExpiration));
    }

    /// <summary>
    /// Adds and configures the <see cref="FusionCache"/> and <see cref="IOltHybridCache"/> hybrid cache services with a specified default absolute expiration to the dependency
    /// injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the hybrid cache services will be added. Cannot be null.</param>
    /// <param name="defaultAbsoluteExpiration">The default absolute expiration date and time to use for <see cref="IMemoryCache"/>.</param>
    /// <param name="config">An action to configure the <see cref="IFusionCacheBuilder"/> for additional cache customization. Cannot be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddOltHybridCache(this IServiceCollection services, Action<IFusionCacheBuilder> builderAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builderAction);
        services.TryAddSingleton<IOltHybridCache, OltHybridCache>();
        var config = services.AddFusionCache();
        builderAction(config);        
        return services;
    }

   
}
