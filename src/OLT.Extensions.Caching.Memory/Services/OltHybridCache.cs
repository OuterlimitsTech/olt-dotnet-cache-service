using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Core;

public class OltHybridCache : IOltHybridCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IFusionCache _cache;

    public OltHybridCache(IFusionCache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Used during bootstraping
    /// </summary>
    public static FusionCacheEntryOptions DefaultOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(5),
        Priority = CacheItemPriority.Normal,
        IsFailSafeEnabled = true,
        EagerRefreshThreshold = 0.5f
    };

    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan? duration = null, CacheItemPriority? cacheItemPriority = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var options = duration.HasValue || cacheItemPriority.HasValue ? _cache.CreateEntryOptions() : _cache.DefaultEntryOptions;

        if (duration.HasValue)
            options.SetDuration(duration.Value);

        if (cacheItemPriority.HasValue)
            options.SetPriority(cacheItemPriority.Value);

        await _cache.SetAsync(key, value, token: cancellationToken);
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        TValue? defaultValue = default;
        return await _cache.GetOrDefaultAsync(key, defaultValue, token: cancellationToken);
    }

    public async Task<TValue> GetOrSetAsync<TValue>(string key, Func<IServiceProvider, IOltHybridCacheContext<TValue>, Task<TValue>> factory, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var options = duration.HasValue ? _cache.CreateEntryOptions(duration: duration) : _cache.DefaultEntryOptions;


        return await _cache.GetOrSetAsync<TValue>(
           key,
            async (ctx, ct) =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                return await factory(scope.ServiceProvider, new OltHybridCacheContext<TValue>(ctx, ct));
            },
           options,
           cancellationToken);

    }


    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        await _cache.RemoveAsync(key, DefaultOptions, cancellationToken);
    }


}
