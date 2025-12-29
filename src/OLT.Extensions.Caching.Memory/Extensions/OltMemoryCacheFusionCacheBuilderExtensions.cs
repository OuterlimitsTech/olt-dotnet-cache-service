using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Core;

public static class OltMemoryCacheFusionCacheBuilderExtensions
{
    /// <summary>
    /// Adds Memory Cache 
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IMemoryCache"/> and <see cref="IOltMemoryCache"/> service.
    /// </remarks>
    /// <param name="builder"><seealso cref="IFusionCacheBuilder"/></param>
    /// <param name="defaultAbsoluteExpiration">Default expire cache at. (uses default if not supplied)</param>
    /// <returns><seealso cref="IFusionCacheBuilder"/></returns>
    public static IFusionCacheBuilder AddOltCacheMemory(this IFusionCacheBuilder builder, TimeSpan defaultAbsoluteExpiration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AddOltCacheMemory(builder, opt => new MemoryCacheEntryOptions().SetAbsoluteExpiration(defaultAbsoluteExpiration));
    }

    /// <summary>
    /// Adds Memory Cache 
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IMemoryCache"/> and <see cref="IOltMemoryCache"/> service.
    /// </remarks>
    /// <param name="builder"><seealso cref="IFusionCacheBuilder"/></param>
    /// <param name="setupAction">Default expire cache at. (uses default if not supplied)</param>
    /// <returns><seealso cref="IFusionCacheBuilder"/></returns>
    public static IFusionCacheBuilder AddOltCacheMemory(this IFusionCacheBuilder builder, Action<MemoryCacheOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setupAction);

        builder.Services.TryAddSingleton<IOltMemoryCache, OltMemoryCache>();
        builder.Services.TryAddSingleton<IOltCacheService>((sp) => sp.GetRequiredService<IOltMemoryCache>());
        builder.Services.AddMemoryCache(setupAction);
        return builder;
    }
}
