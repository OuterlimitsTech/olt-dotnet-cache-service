using Microsoft.Extensions.Caching.Memory;

namespace OLT.Core;

public interface IOltHybridCache
{

    /// <summary>
    /// The indenpent scoped <see cref="IServiceProvider"/> must be used to handle Database Context threads
    /// </summary>
    /// <remarks>
    /// https://github.com/ZiggyCreatures/FusionCache/issues/477
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="duration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> GetOrSetAsync<T>(string key, Func<IServiceProvider, IOltHybridCacheContext<T>, Task<T>> factory, TimeSpan? duration = null, CancellationToken cancellationToken = default);

    Task SetAsync<TValue>(string key, TValue value, TimeSpan? duration = null, CacheItemPriority? cacheItemPriority = null, CancellationToken cancellationToken = default);
    Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

}

