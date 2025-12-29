using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Core;

public class OltHybridCacheContext<TValue> : IOltHybridCacheContext<TValue>
{
    public OltHybridCacheContext(FusionCacheFactoryExecutionContext<TValue> fusionCache, CancellationToken cancellationToken)  //FusionCacheFactoryExecutionContext<TValue> fusionCache,
    {
        FusionCache = fusionCache;
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }
    private FusionCacheFactoryExecutionContext<TValue> FusionCache { get; }

    public void SetPriority(CacheItemPriority priority)
    {
        FusionCache.Options.SetPriority(priority);
    }

    public void SetDuration(TimeSpan duration)
    {
        FusionCache.Options.SetDuration(duration);
    }

    
}
