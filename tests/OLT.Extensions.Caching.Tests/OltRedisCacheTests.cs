using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using OLT.Core;
//using StackExchange.Redis.Extensions.Core.Configuration;
using Testcontainers.Redis;

namespace OLT.Extensions.Caching.Tests;

public class OltRedisCacheTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;

    public OltRedisCacheTests()
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

    [Fact]
    public void Get_ReturnsValueFromRedis()
    {
        var key = "testKey";
        var value = "testValue";
        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        cacheService.Get(key, () => value);

        var result = cacheService.Get(key, () => "defaultValue");

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsValueFromRedis()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        await cacheService.GetAsync(key, () => Task.FromResult(value));

        var result = await cacheService.GetAsync(key, () => Task.FromResult("defaultValue"));

        Assert.Equal(value, result);
    }

    [Fact]
    public void Remove_RemovesValueFromRedis()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        cacheService.Get(key, () => value);
        cacheService.Remove(key);

        var result = cacheService.Exists(key);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveAsync_RemovesValueFromRedis()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        await cacheService.GetAsync(key, () => Task.FromResult(value));
        await cacheService.RemoveAsync(key);

        var result = await cacheService.ExistsAsync(key);

        Assert.False(result);
    }

    [Fact]
    public void Exists_ReturnsTrueWhenValueExistsInRedis()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        cacheService.Get(key, () => value);

        var result = cacheService.Exists(key);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueWhenValueExistsInRedis()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();


        await cacheService.GetAsync(key, () => Task.FromResult(value));

        var result = await cacheService.ExistsAsync(key);

        Assert.True(result);
    }

    [Fact]
    public void Flush_CallsFlushAsync()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();


        cacheService.Get(key, () => value);
        cacheService.Flush();

        var result = cacheService.Exists(key);

        Assert.False(result);
    }

    [Fact]
    public async Task FlushAsync_CallsFlushAsync()
    {
        var key = "testKey";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(4), "prefix:");
        var cacheService = provider.GetRequiredService<IOltCacheService>();


        await cacheService.GetAsync(key, () => Task.FromResult(value));
        await cacheService.FlushAsync();

        var result = await cacheService.ExistsAsync(key);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAsync_ReturnsValueWithinTimeout()
    {
        var key = $"testKey:{Guid.NewGuid()}";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(2), "TimeoutAsyncTests");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        var result = await cacheService.GetAsync(key, () => Task.FromResult(value), TimeSpan.FromSeconds(1));
        result.Should().BeEquivalentTo(value);
        Assert.False(new ManualResetEvent(false).WaitOne(1500));

        result = await cacheService.GetAsync(key, () => Task.FromResult(Guid.NewGuid().ToString()), TimeSpan.FromSeconds(1));
        result.Should().NotBeEquivalentTo(value);

    }

    [Fact]
    public void Get_ReturnsValueWithinTimeout()
    {
        var key = $"testKey:{Guid.NewGuid()}";
        var value = "testValue";

        var provider = TestHelper.BuildRedisProvider(_redisContainer, TimeSpan.FromSeconds(2), "TimeoutAsyncTests");
        var cacheService = provider.GetRequiredService<IOltCacheService>();

        var result = cacheService.Get(key, () => value, TimeSpan.FromSeconds(1));
        result.Should().BeEquivalentTo(value);
        Assert.False(new ManualResetEvent(false).WaitOne(1500));

        result = cacheService.Get(key, () => Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1));
        result.Should().NotBeEquivalentTo(value);
    }

    //[Fact]
    //public void AddOltCacheRedis_ThrowsArgumentNullException()
    //{
    //    var services = new ServiceCollection();
    //    string? connectionString = null;
    //    string cacheKeyPrefix = "test-app";

    //    Assert.Throws<ArgumentNullException>("services", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(null, TimeSpan.FromSeconds(15), cacheKeyPrefix, "abc-123"));
    //    Assert.Throws<ArgumentNullException>("services", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(null, TimeSpan.FromSeconds(15), null, "abc-123"));
    //    Assert.Throws<ArgumentNullException>("services", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(null, TimeSpan.FromSeconds(15), "", "abc-123"));

    //    Assert.Throws<ArgumentException>("cacheKeyPrefix", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), "", "abc-123"));
    //    Assert.Throws<ArgumentException>("cacheKeyPrefix", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), " ", "abc-123"));
    //    Assert.Throws<ArgumentNullException>("cacheKeyPrefix", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), null, "abc-123"));

    //    Assert.Throws<ArgumentException>("connectionString", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(null, TimeSpan.FromSeconds(15), cacheKeyPrefix, ""));
    //    Assert.Throws<ArgumentNullException>("connectionString", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), cacheKeyPrefix, connectionString));
    //    Assert.Throws<ArgumentException>("connectionString", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), cacheKeyPrefix, ""));
    //    Assert.Throws<ArgumentException>("connectionString", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), cacheKeyPrefix, " "));

    //    StackExchange.Redis.Extensions.Core.Configuration.RedisConfiguration options = null;
    //    Assert.Throws<ArgumentNullException>("redisConfiguration", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(services, TimeSpan.FromSeconds(15), cacheKeyPrefix, options));

    //    var host = new List<RedisHost>();
    //    host.Add(new RedisHost { Host = "localhost", Port = 6379 });
    //    options = new StackExchange.Redis.Extensions.Core.Configuration.RedisConfiguration();
    //    options.Hosts = host.ToArray();
    //    Assert.Throws<ArgumentNullException>("services", () => OltRedisCacheServiceCollectionExtensions.AddOltCacheRedis(null, TimeSpan.FromSeconds(15), cacheKeyPrefix, options));

    //}

}
