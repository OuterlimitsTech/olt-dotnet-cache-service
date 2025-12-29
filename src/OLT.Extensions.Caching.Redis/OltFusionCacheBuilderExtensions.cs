//using Microsoft.Extensions.Caching.StackExchangeRedis;
//using StackExchange.Redis;
//using ZiggyCreatures.Caching.Fusion;
//using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

//namespace OLT.Core
//{
//    public static class OltFusionCacheBuilderExtensions
//    {

//        public static IFusionCacheBuilder WithRedisBackplane(this IFusionCacheBuilder cacheBuilder, OltRedisCacheOptions options, Action<ConfigurationOptions>? config = null)
//        {

//            var redisBackplaneOptions = new RedisBackplaneOptions
//            {
//                ConfigurationOptions = ConfigurationOptions.Parse(options.RedisConnectionString)
//            };


//            if (config != null)
//            {
//                config(redisBackplaneOptions.ConfigurationOptions);
//            }

//            cacheBuilder.WithBackplane(new RedisBackplane(redisBackplaneOptions));
//            cacheBuilder.WithCacheKeyPrefix(options.CacheKeyPrefix);

//            return cacheBuilder;

//        }

//        public static IFusionCacheBuilder WithRedisDistributedCache(this IFusionCacheBuilder cacheBuilder, OltRedisCacheOptions options) //, Action<RedisCacheOptions>? config = null)
//        {
//            var redisCacheOptions = new RedisCacheOptions
//            {
//                ConfigurationOptions = ConfigurationOptions.Parse(options.RedisConnectionString),
//                InstanceName = options.CacheKeyPrefix,               
//            };

//            //if (config != null)
//            //{
//            //    config(options);
//            //}

//            var test = new RedisCache(redisCacheOptions)
//            {
                
//            };
//            cacheBuilder.WithDistributedCache(test);

//            return cacheBuilder;
//        }
//    }
//}
