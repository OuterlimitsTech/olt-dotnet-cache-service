[![CI](https://github.com/OuterlimitsTech/olt-dotnet-core/actions/workflows/build.yml/badge.svg)](https://github.com/OuterlimitsTech/olt-dotnet-core/actions/workflows/build.yml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=OuterlimitsTech_olt-dotnet-core&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=OuterlimitsTech_olt-dotnet-core)

## OLT Server Memory Caching 


```csharp
services.AddOltCacheMemory(TimeSpan.FromMinutes(30))


//Then inject IOltCacheService

private readonly IOltCacheService _cacheService;
```


### Sync
```csharp
var clients = _memoryCache.Get("Clients", () => Context.Cleints.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ThenBy(p => p.Id).ToList(), TimeSpan.FromMinutes(30));
```

### Async
```csharp
var clients = await _cacheService.GetAsync("Clients", async () => await Context.Cleints.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ThenBy(p => p.Id).ToListAsync(), TimeSpan.FromMinutes(30));
```


### Remove
```csharp
_cacheService.Remove("Clients");
```