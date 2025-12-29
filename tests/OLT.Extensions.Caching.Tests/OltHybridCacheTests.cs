using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLT.Core;
using Testcontainers.Redis;
using ZiggyCreatures.Caching.Fusion;

namespace OLT.Extensions.Caching.Tests;

public class OltHybridCacheTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;

    public OltHybridCacheTests()
    {
        _redisContainer = new RedisBuilder().Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.StopAsync();
    }

    private ServiceProvider BuildProvider()
    {
        var connStr = $"{_redisContainer.GetConnectionString()},allowAdmin=true,defaultDatabase=3";
        var services = new ServiceCollection();
        services.AddLogging(config => config.AddConsole());
        services.AddOltHybridCache(config =>
        {
            config.WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal,
                IsFailSafeEnabled = true,
                EagerRefreshThreshold = 0.5f
            })
            .WithCacheKeyPrefix("OLT:UnitTest:")            
            .WithSystemTextJsonSerializer()
            .WithDistributedCache(new RedisCache(new RedisCacheOptions { Configuration = connStr }))
            //.WithBackplane(new RedisBackplane(new RedisBackplaneOptions { Configuration = connStr }))                        
            .WithStackExchangeRedisBackplane(opt => opt.Configuration = connStr)
            ;
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCacheIsNull()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var serviceScopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        Assert.Throws<ArgumentNullException>(() => new OltHybridCache(null!, serviceScopeFactory));
    }

    [Fact]
    public void DefaultOptions_ReturnsCorrectConfiguration()
    {
        var options = OltHybridCache.DefaultOptions;

        Assert.Equal(TimeSpan.FromMinutes(5), options.Duration);
        Assert.Equal(CacheItemPriority.Normal, options.Priority);
        Assert.True(options.IsFailSafeEnabled);
        Assert.Equal(0.5f, options.EagerRefreshThreshold);
    }

    [Fact]
    public async Task SetAsync_ThrowsArgumentException_WhenKeyIsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_ThrowsArgumentException_WhenKeyIsEmpty()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.SetAsync(string.Empty, "value"));
    }

    [Fact]
    public async Task SetAsync_ThrowsArgumentException_WhenKeyIsWhitespace()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.SetAsync("   ", "value"));
    }

    [Fact]
    public async Task SetAsync_SetsValueInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.SetAsync(key, value);

        var result = await cache.GetAsync<string>(key);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_SetsValueWithCustomDuration()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";
        var duration = TimeSpan.FromSeconds(30);

        await cache.SetAsync(key, value, duration);

        var result = await cache.GetAsync<string>(key);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_SetsValueWithCustomPriority()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.SetAsync(key, value, cacheItemPriority: CacheItemPriority.High);

        var result = await cache.GetAsync<string>(key);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_SetsValueWithCustomDurationAndPriority()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";
        var duration = TimeSpan.FromSeconds(30);

        await cache.SetAsync(key, value, duration, CacheItemPriority.Low);

        var result = await cache.GetAsync<string>(key);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingValue()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var firstValue = "firstValue";
        var secondValue = "secondValue";

        await cache.SetAsync(key, firstValue);
        await cache.SetAsync(key, secondValue);

        var result = await cache.GetAsync<string>(key);
        Assert.Equal(secondValue, result);
    }

    [Fact]
    public async Task SetAsync_HandlesCancellation()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cache.SetAsync("testKey", "testValue", cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentException_WhenKeyIsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.GetAsync<string>(null!));
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentException_WhenKeyIsEmpty()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.GetAsync<string>(string.Empty));
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentException_WhenKeyIsWhitespace()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>  await cache.GetAsync<string>("   "));
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var result = await cache.GetAsync<string>("nonExistentKey");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.SetAsync(key, value);
        var result = await cache.GetAsync<string>(key);

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_WorksWithDifferentTypes()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await cache.SetAsync("intKey", 42);
        await cache.SetAsync("stringKey", "test");
        await cache.SetAsync("objectKey", new { Id = 1, Name = "Test" });

        var intValue = await cache.GetAsync<int>("intKey");
        var stringValue = await cache.GetAsync<string>("stringKey");
        var objectValue = await cache.GetAsync<dynamic>("objectKey");

        Assert.Equal(42, intValue);
        Assert.Equal("test", stringValue);
        Assert.Equal(1, objectValue.Id);
        Assert.Equal("Test", objectValue.Name);
    }

    [Fact]
    public async Task GetAsync_HandlesCancellation()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cache.GetAsync<string>("testKey", cts.Token));
    }

    [Fact]
    public async Task GetOrSetAsync_ThrowsArgumentException_WhenKeyIsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.GetOrSetAsync<string>(null!, (sp, ctx) => Task.FromResult("value")));
    }

    [Fact]
    public async Task GetOrSetAsync_ThrowsArgumentException_WhenKeyIsEmpty()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.GetOrSetAsync<string>(string.Empty, (sp, ctx) => Task.FromResult("value")));
    }

    [Fact]
    public async Task GetOrSetAsync_ThrowsArgumentException_WhenKeyIsWhitespace()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.GetOrSetAsync<string>("   ", (sp, ctx) => Task.FromResult("value")));
    }

    [Fact]
    public async Task GetOrSetAsync_CallsFactory_WhenKeyDoesNotExist()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var expectedValue = "testValue";
        var factoryCalled = false;

        var result = await cache.GetOrSetAsync<string>(key, (sp, ctx) =>
        {
            factoryCalled = true;
            return Task.FromResult(expectedValue);
        });

        Assert.True(factoryCalled);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetOrSetAsync_DoesNotCallFactory_WhenKeyExists()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var firstValue = "firstValue";
        var secondValue = "secondValue";
        var factoryCallCount = 0;

        await cache.GetOrSetAsync<string>(key, (sp, ctx) =>
        {
            factoryCallCount++;
            return Task.FromResult(firstValue);
        });

        var result = await cache.GetOrSetAsync<string>(key, (sp, ctx) =>
        {
            factoryCallCount++;
            return Task.FromResult(secondValue);
        });

        Assert.Equal(1, factoryCallCount);
        Assert.Equal(firstValue, result);
    }

    [Fact]
    public async Task GetOrSetAsync_ProvidesServiceProvider_ToFactory()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        IServiceProvider? capturedProvider = null;

        await cache.GetOrSetAsync<string>("testKey", (sp, ctx) =>
        {
            capturedProvider = sp;
            return Task.FromResult("value");
        });

        Assert.NotNull(capturedProvider);
    }

    [Fact]
    public async Task GetOrSetAsync_ProvidesCacheContext_ToFactory()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        IOltHybridCacheContext<string>? capturedContext = null;

        await cache.GetOrSetAsync<string>("testKey", (sp, ctx) =>
        {
            capturedContext = ctx;
            return Task.FromResult("value");
        });

        Assert.NotNull(capturedContext);
    }

    [Fact]
    public async Task GetOrSetAsync_UsesCustomDuration()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";
        var duration = TimeSpan.FromSeconds(30);

        var result = await cache.GetOrSetAsync<string>(key, (sp, ctx) => Task.FromResult(value), duration);

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetOrSetAsync_AllowsSettingPriorityInContext()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        var result = await cache.GetOrSetAsync<string>(key, (sp, ctx) =>
        {
            ctx.SetPriority(CacheItemPriority.High);
            return Task.FromResult(value);
        });

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetOrSetAsync_AllowsSettingDurationInContext()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        var result = await cache.GetOrSetAsync<string>(key, (sp, ctx) =>
        {
            ctx.SetDuration(TimeSpan.FromMinutes(10));
            return Task.FromResult(value);
        });

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetOrSetAsync_HandlesCancellation()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cache.GetOrSetAsync<string>("testKey", (sp, ctx) => Task.FromResult("value"), cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetOrSetAsync_WorksWithDifferentTypes()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var intValue = await cache.GetOrSetAsync<int>("intKey", (sp, ctx) => Task.FromResult(42));
        var stringValue = await cache.GetOrSetAsync<string>("stringKey", (sp, ctx) => Task.FromResult("test"));
        var testObj = new TestObject { Id = 1, Name = "Test" };
        var complexValue = await cache.GetOrSetAsync<TestObject>("complexKey", (sp, ctx) => Task.FromResult(testObj));

        Assert.Equal(42, intValue);
        Assert.Equal("test", stringValue);
        Assert.Equal(1, complexValue.Id);
        Assert.Equal("Test", complexValue.Name);
    }

    [Fact]
    public async Task GetOrSetAsync_CanAccessScopedServices()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddFusionCache()
            .WithDefaultEntryOptions(OltHybridCache.DefaultOptions);
        services.AddScoped<TestScopedService>();
        services.AddSingleton<IOltHybridCache, OltHybridCache>();
        var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<IOltHybridCache>();

        var result = await cache.GetOrSetAsync<string>("testKey", (sp, ctx) =>
        {
            var scopedService = sp.GetRequiredService<TestScopedService>();
            return Task.FromResult(scopedService.GetValue());
        });

        Assert.Equal("ScopedValue", result);
    }

    [Fact]
    public async Task RemoveAsync_ThrowsArgumentException_WhenKeyIsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.RemoveAsync(null!));
    }

    [Fact]
    public async Task RemoveAsync_ThrowsArgumentException_WhenKeyIsEmpty()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.RemoveAsync(string.Empty));
    }

    [Fact]
    public async Task RemoveAsync_ThrowsArgumentException_WhenKeyIsWhitespace()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await cache.RemoveAsync("   "));
    }

    [Fact]
    public async Task RemoveAsync_RemovesValueFromCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.SetAsync(key, value);
        var beforeRemove = await cache.GetAsync<string>(key);
        Assert.Equal(value, beforeRemove);

        await cache.RemoveAsync(key);
        var afterRemove = await cache.GetAsync<string>(key);
        Assert.Null(afterRemove);
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow_WhenKeyDoesNotExist()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var exception = await Record.ExceptionAsync(async () =>
            await cache.RemoveAsync("nonExistentKey"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RemoveAsync_HandlesCancellation()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cache.RemoveAsync("testKey", cts.Token));
    }

    [Fact]
    public async Task MultipleOperations_WorkCorrectly()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");
        await cache.SetAsync("key3", "value3");

        var result1 = await cache.GetAsync<string>("key1");
        var result2 = await cache.GetAsync<string>("key2");
        var result3 = await cache.GetAsync<string>("key3");

        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);
        Assert.Equal("value3", result3);

        await cache.RemoveAsync("key2");

        var afterRemove1 = await cache.GetAsync<string>("key1");
        var afterRemove2 = await cache.GetAsync<string>("key2");
        var afterRemove3 = await cache.GetAsync<string>("key3");

        Assert.Equal("value1", afterRemove1);
        Assert.Null(afterRemove2);
        Assert.Equal("value3", afterRemove3);
    }

    [Fact]
    public async Task CacheContext_CancellationToken_IsProvided()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltHybridCache>();

        var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        await cache.GetOrSetAsync<string>("testKey", (sp, ctx) =>
        {
            capturedToken = ctx.CancellationToken;
            return Task.FromResult("value");
        }, cancellationToken: cts.Token);

        Assert.NotEqual(default, capturedToken);
    }

    private class TestScopedService
    {
        public string GetValue() => "ScopedValue";
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
