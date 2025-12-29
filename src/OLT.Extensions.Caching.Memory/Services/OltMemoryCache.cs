using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Core;

public class OltMemoryCache : IOltMemoryCache
{
    private readonly IMemoryCache _memoryCache;

    public OltMemoryCache(
        IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;            
    }

    protected virtual string ToCacheKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return key.ToLower();
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(ToCacheKey(key));
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(ToCacheKey(key));
        return Task.CompletedTask;
    }

    public TEntry Get<TEntry>(string key, Func<TEntry> factory, TimeSpan? absoluteExpiration = null)
    {            
        var cacheEntry = _memoryCache.GetOrCreate(ToCacheKey(key), entry =>
        {
            if (absoluteExpiration.HasValue)
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.Add(absoluteExpiration.Value);
            }
            return factory();
        });

        return cacheEntry ?? throw new NullReferenceException("Cache Factory returned null");
    }

    public async Task<TEntry> GetAsync<TEntry>(string key, Func<Task<TEntry>> factory, TimeSpan? absoluteExpiration = null)
    {
        var cacheEntry = await
              _memoryCache.GetOrCreateAsync(ToCacheKey(key), async entry =>
              {
                  if (absoluteExpiration.HasValue)
                  {
                      entry.AbsoluteExpiration = DateTimeOffset.Now.Add(absoluteExpiration.Value);
                  }
                  return await factory();
              });
        return cacheEntry ?? throw new NullReferenceException("Cache Factory returned null");
    }

    public bool Exists(string key)
    {
        if (_memoryCache.TryGetValue(ToCacheKey(key), out object? value))
        {
            return true;
        }
        return false;
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (_memoryCache.TryGetValue(ToCacheKey(key), out object? value))
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public void Flush()
    {
        if (_memoryCache is MemoryCache memoryCache)
        {
            var percentage = 1.0; //100%
            memoryCache.Compact(percentage);
        }
    }

    public Task FlushAsync()
    {
        Flush();
        return Task.CompletedTask;
    }
}
