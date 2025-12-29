using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OLT.Core;

namespace OLT.Extensions.Caching.Tests;

public class OltMemoryCacheTests
{
    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<IOltMemoryCache, OltMemoryCache>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void ToCacheKey_ThrowsArgumentException_WhenKeyIsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        Assert.Throws<ArgumentNullException>(() => cache.Get<string>(null!, () => "value"));
    }

    [Fact]
    public void ToCacheKey_ThrowsArgumentException_WhenKeyIsEmpty()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        Assert.Throws<ArgumentException>(() => cache.Get<string>(string.Empty, () => "value"));
    }

    [Fact]
    public void ToCacheKey_ConvertsKeyToLowerCase()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var upperKey = "TESTKEY";
        var lowerKey = "testkey";

        cache.Get(upperKey, () => "value1");
        var result = cache.Get(lowerKey, () => "value2");

        Assert.Equal("value1", result);
    }

    [Fact]
    public void Get_ReturnsValueFromFactory_WhenNotInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var expectedValue = "testValue";
        var result = cache.Get(key, () => expectedValue);

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Get_ReturnsCachedValue_WhenAlreadyInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var firstValue = "firstValue";
        var secondValue = "secondValue";

        cache.Get(key, () => firstValue);
        var result = cache.Get(key, () => secondValue);

        Assert.Equal(firstValue, result);
    }

    [Fact]
    public void Get_ThrowsNullReferenceException_WhenFactoryReturnsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        Assert.Throws<NullReferenceException>(() => cache.Get<string>(key, () => null!));
    }

    [Fact]
    public void Get_SetsAbsoluteExpiration_WhenProvided()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();
        var memoryCache = provider.GetRequiredService<IMemoryCache>();

        var key = "testKey";
        var value = "testValue";
        var expiration = TimeSpan.FromSeconds(10);

        cache.Get(key, () => value, expiration);

        Assert.True(cache.Exists(key));
    }

    [Fact]
    public async Task GetAsync_ReturnsValueFromFactory_WhenNotInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var expectedValue = "testValue";
        var result = await cache.GetAsync(key, () => Task.FromResult(expectedValue));

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsCachedValue_WhenAlreadyInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var firstValue = "firstValue";
        var secondValue = "secondValue";

        await cache.GetAsync(key, () => Task.FromResult(firstValue));
        var result = await cache.GetAsync(key, () => Task.FromResult(secondValue));

        Assert.Equal(firstValue, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNullReferenceException_WhenFactoryReturnsNull()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        await Assert.ThrowsAsync<NullReferenceException>(() => 
            cache.GetAsync<string>(key, () => Task.FromResult<string>(null!)));
    }

    [Fact]
    public async Task GetAsync_SetsAbsoluteExpiration_WhenProvided()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var value = "testValue";
        var expiration = TimeSpan.FromSeconds(10);

        await cache.GetAsync(key, () => Task.FromResult(value), expiration);

        Assert.True(await cache.ExistsAsync(key));
    }

    [Fact]
    public void Remove_RemovesValueFromCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var value = "testValue";

        cache.Get(key, () => value);
        Assert.True(cache.Exists(key));

        cache.Remove(key);
        Assert.False(cache.Exists(key));
    }

    [Fact]
    public void Remove_DoesNotThrow_WhenKeyDoesNotExist()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "nonExistentKey";
        var exception = Record.Exception(() => cache.Remove(key));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RemoveAsync_RemovesValueFromCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.GetAsync(key, () => Task.FromResult(value));
        Assert.True(await cache.ExistsAsync(key));

        await cache.RemoveAsync(key);
        Assert.False(await cache.ExistsAsync(key));
    }

    [Fact]
    public async Task RemoveAsync_DoesNotThrow_WhenKeyDoesNotExist()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "nonExistentKey";
        var exception = await Record.ExceptionAsync(async () => await cache.RemoveAsync(key));

        Assert.Null(exception);
    }

    [Fact]
    public void Exists_ReturnsTrue_WhenKeyExistsInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var value = "testValue";

        cache.Get(key, () => value);
        var result = cache.Exists(key);

        Assert.True(result);
    }

    [Fact]
    public void Exists_ReturnsFalse_WhenKeyDoesNotExistInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "nonExistentKey";
        var result = cache.Exists(key);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenKeyExistsInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "testKey";
        var value = "testValue";

        await cache.GetAsync(key, () => Task.FromResult(value));
        var result = await cache.ExistsAsync(key);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenKeyDoesNotExistInCache()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var key = "nonExistentKey";
        var result = await cache.ExistsAsync(key);

        Assert.False(result);
    }

    [Fact]
    public void Flush_RemovesAllCachedEntries()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        cache.Get("key1", () => "value1");
        cache.Get("key2", () => "value2");
        cache.Get("key3", () => "value3");

        Assert.True(cache.Exists("key1"));
        Assert.True(cache.Exists("key2"));
        Assert.True(cache.Exists("key3"));

        cache.Flush();

        Assert.False(cache.Exists("key1"));
        Assert.False(cache.Exists("key2"));
        Assert.False(cache.Exists("key3"));
    }

    [Fact]
    public async Task FlushAsync_RemovesAllCachedEntries()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        await cache.GetAsync("key1", () => Task.FromResult("value1"));
        await cache.GetAsync("key2", () => Task.FromResult("value2"));
        await cache.GetAsync("key3", () => Task.FromResult("value3"));

        Assert.True(await cache.ExistsAsync("key1"));
        Assert.True(await cache.ExistsAsync("key2"));
        Assert.True(await cache.ExistsAsync("key3"));

        await cache.FlushAsync();

        Assert.False(await cache.ExistsAsync("key1"));
        Assert.False(await cache.ExistsAsync("key2"));
        Assert.False(await cache.ExistsAsync("key3"));
    }

    [Fact]
    public void Get_WorksWithDifferentTypes()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var intValue = cache.Get("intKey", () => 42);
        var stringValue = cache.Get("stringKey", () => "test");
        var complexValue = cache.Get("complexKey", () => new { Id = 1, Name = "Test" });

        Assert.Equal(42, intValue);
        Assert.Equal("test", stringValue);
        Assert.Equal(1, complexValue.Id);
        Assert.Equal("Test", complexValue.Name);
    }

    [Fact]
    public async Task GetAsync_WorksWithDifferentTypes()
    {
        var provider = BuildProvider();
        var cache = provider.GetRequiredService<IOltMemoryCache>();

        var intValue = await cache.GetAsync("intKey", () => Task.FromResult(42));
        var stringValue = await cache.GetAsync("stringKey", () => Task.FromResult("test"));
        var complexValue = await cache.GetAsync("complexKey", () => Task.FromResult(new { Id = 1, Name = "Test" }));

        Assert.Equal(42, intValue);
        Assert.Equal("test", stringValue);
        Assert.Equal(1, complexValue.Id);
        Assert.Equal("Test", complexValue.Name);
    }

    [Fact]
    public void AddOltCacheMemory_ThrowsArgumentNullException_WhenServicesIsNull()
    {
#pragma warning disable CS0618
        Assert.Throws<ArgumentNullException>("services", 
            () => OltMemoryCacheServiceCollectionExtensions.AddOltCacheMemory(null!, TimeSpan.FromSeconds(15)));
#pragma warning restore CS0618
    }

    [Fact]
    public void AddOltHybridCache_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>("services",
            () => OltMemoryCacheServiceCollectionExtensions.AddOltHybridCache(null!, cfg => { }));
    }

    [Fact]
    public void AddOltHybridCache_ThrowsArgumentNullException_WhenBuilderActionIsNull()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>("builderAction",
            () => OltMemoryCacheServiceCollectionExtensions.AddOltHybridCache(services, null!));
    }
}
