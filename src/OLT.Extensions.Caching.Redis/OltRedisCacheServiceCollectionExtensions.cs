using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace OLT.Core;


public static class OltRedisCacheServiceCollectionExtensions
{


    /// <summary>
    /// Adds Redis Cache Service
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IOltCacheService"/> as a singleton to <see cref="OltRedisCache"/> and <see cref="RedisConfiguration"/>
    /// </remarks>
    /// <param name="services"><seealso cref="IServiceCollection"/></param>
    /// <param name="defaultAbsoluteExpiration">Default expire cache.</param>
    /// <param name="cacheKeyPrefix">Keyprefix for namespacing cache entries</param>
    /// <param name="connectionString"> The string configuration to use for this <see cref="RedisConfiguration.ConnectionString"/>. ("localhost:6379,serviceName=application2,name=my-test-app2,defaultDatabase=3")</param>
    /// <returns><seealso cref="IServiceCollection"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    [Obsolete("Removing in 10.x, Move to IOltHybridCache")]
    public static IServiceCollection AddOltCacheRedis(this IServiceCollection services, TimeSpan defaultAbsoluteExpiration, string cacheKeyPrefix, string connectionString)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(connectionString);
        return AddOltCacheRedis(services, defaultAbsoluteExpiration, cacheKeyPrefix, new RedisConfiguration { ConnectionString = connectionString });
    }

    /// <summary>
    /// Adds Redis Cache Service
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IOltCacheService"/> as a singleton to <see cref="OltRedisCache"/> and <see cref="RedisConfiguration"/>
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="defaultAbsoluteExpiration"></param>
    /// <param name="cacheKeyPrefix">Keyprefix for namespacing cache entries</param>
    /// <param name="redisConfiguration"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    [Obsolete("Removing in 10.x, Move to IOltHybridCache")]
    public static IServiceCollection AddOltCacheRedis(this IServiceCollection services, TimeSpan defaultAbsoluteExpiration, string cacheKeyPrefix, RedisConfiguration redisConfiguration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(redisConfiguration);
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKeyPrefix);

        redisConfiguration.Name = redisConfiguration.Name ?? redisConfiguration.ConfigurationOptions.ClientName;
        redisConfiguration.AllowAdmin = true;

        services.AddSingleton((provider) => provider.GetRequiredService<IRedisClientFactory>().GetDefaultRedisClient());
        services.AddSingleton((provider) => provider.GetRequiredService<IRedisClientFactory>().GetDefaultRedisClient().GetDefaultDatabase());

        services
            .AddMemoryCache(opt => new MemoryCacheEntryOptions().SetAbsoluteExpiration(defaultAbsoluteExpiration))
            .AddSingleton<IOltMemoryCache, OltMemoryCache>();

        return services
            .AddSingleton<IOltCacheService, OltRedisCache>()
            .AddSingleton(redisConfiguration)
            .AddSingleton<IRedisClientFactory, RedisClientFactory>()
            .AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .Configure<OltRedisCacheOptions>(opt =>
            {
                opt.CacheKeyPrefix = cacheKeyPrefix.ToLower();
                opt.DefaultAbsoluteExpiration = defaultAbsoluteExpiration;
            });
    }


}
