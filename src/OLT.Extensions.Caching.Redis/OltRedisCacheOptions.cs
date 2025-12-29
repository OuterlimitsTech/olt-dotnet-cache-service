using System.Security.Cryptography;

namespace OLT.Core;

[Obsolete("Removing in 10.x, Move to IOltHybridCache or IOltMemoryCache")]
public class OltRedisCacheOptions 
{

    /// <summary>
    /// Used for distributed cache to prevent collisions with other applications.
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "OLT:NotSet:";
    
    public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromSeconds(1);
 
}
