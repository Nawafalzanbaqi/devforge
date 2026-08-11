using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using DevForge.ControlPlane.Application.Common.Interfaces;
using StackExchange.Redis;

namespace DevForge.ControlPlane.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;

    public CacheService(IDistributedCache cache, IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            if (data == null) return default;
            return JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            // Graceful degradation on Redis cache timeouts or failures
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }
        catch
        {
            // Graceful degradation: ignore save failures
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch
        {
            // Graceful degradation: ignore remove failures
        }
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        if (_redis != null && _redis.IsConnected)
        {
            var db = _redis.GetDatabase();
            var payload = JsonSerializer.Serialize(message);
            await db.PublishAsync(RedisChannel.Literal(channel), payload);
        }
    }

    public async Task SubscribeAsync<T>(string channel, Func<T, Task> handler)
    {
        if (_redis != null && _redis.IsConnected)
        {
            var sub = _redis.GetSubscriber();
            await sub.SubscribeAsync(RedisChannel.Literal(channel), async (chan, val) =>
            {
                var valString = val.ToString();
                if (!string.IsNullOrEmpty(valString))
                {
                    var message = JsonSerializer.Deserialize<T>(valString);
                    if (message != null)
                    {
                        await handler(message);
                    }
                }
            });
        }
    }
}
