[![CI](https://github.com/OuterlimitsTech/olt-dotnet-cache-service/actions/workflows/build.yml/badge.svg)](https://github.com/OuterlimitsTech/olt-dotnet-cache-service/actions/workflows/build.yml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=OuterlimitsTech_olt-dotnet-cache-service&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=OuterlimitsTech_olt-dotnet-cache-service)

## OLTMemoryCache

```csharp

//Bootstrap
services.AddOltCacheMemory(TimeSpan.FromMinutes(30))


//Inject Interface
private readonly IOltMemoryCache _memoryCache;


var clients = await _memoryCache.GetAsync("Clients", async () => await Context.Clients.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ThenBy(p => p.Id).ToListAsync(), TimeSpan.FromMinutes(30));

bool exists = await _memoryCache.ExistsAsync("Clients");

await _memoryCache.RemoveAsync("Clients");

await _memoryCache.FlushAsync();  //Clear everything

```




### OltHybridCache

```
Add Packages:

ZiggyCreatures.FusionCache.Backplane.StackExchangeRedis
ZiggyCreatures.FusionCache.Serialization.SystemTextJson
```


```csharp

//Bootstrap
services.AddOltHybridCache(config =>
{
    config.WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(5),
        Priority = CacheItemPriority.Normal,
        IsFailSafeEnabled = true,
        EagerRefreshThreshold = 0.5f
    })
    .WithCacheKeyPrefix("OLT:MyApp:")            
    .WithSystemTextJsonSerializer()
    .WithDistributedCache(new RedisCache(new RedisCacheOptions { Configuration = connStr }))
    .WithStackExchangeRedisBackplane(opt => opt.Configuration = connStr)
    ;
});

//Inject Interface
private readonly IOltHybridCache _hybridCache;

await _hybridCache.SetAsync("my_key", "1234);
var myKeyValue = await _hybridCache.GetAsync<string>("my_key");

var myValues = await _hybridCache.GetOrSetAsync<MyModel>("my_model_key", async (serviceProvider, context) =>
    {
        var repo = serviceProvider.GetRequiredService<IMyRepo>();        
        return await repo.GetAllAsync(context.CancellationToken);
    }, 
    duration: TimeSpan.FromMinutes(5), 
    cancellationToken: cancellationToken);


```

