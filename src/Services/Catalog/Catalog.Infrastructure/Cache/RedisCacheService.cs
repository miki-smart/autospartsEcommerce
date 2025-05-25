using Catalog.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Catalog.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly CacheSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IDistributedCache cache,
            IOptions<CacheSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return default;
                }

                var result = JsonSerializer.Deserialize<T>(cachedValue);
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.AbsoluteExpirationInMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationInMinutes)
                };

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
                _logger.LogInformation("Item cached with key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting item in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("Item removed from cache with key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cache for key: {Key}", key);
            }
        }
    }
}
