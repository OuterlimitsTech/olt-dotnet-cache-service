using Microsoft.Extensions.Caching.Memory;

namespace OLT.Core;

public interface IOltHybridCacheContext<TValue>
{
    CancellationToken CancellationToken { get; }
    void SetPriority(CacheItemPriority priority);
    void SetDuration(TimeSpan duration);
}

