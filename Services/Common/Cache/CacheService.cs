using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace pos_service.Services.Common.Cache
{
    /// <summary>
    /// A cache service that supports primary and optional secondary keys.
    /// Primary key is used as a prefix so RemovePrimary can evict all related entries.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        // maintain a set of keys per primaryKey so we can evict groups efficiently
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _primaryIndex = new();

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private static string ComposeKey(ServiceCacheKey primary, string? secondary)
        {
            var key = primary.ToString();
            if (string.IsNullOrEmpty(secondary))
                return key;

            return key + "_" + secondary.ToLowerInvariant();
        }

        public T? Get<T>(ServiceCacheKey primaryKey, string? secondaryKey = null)
        {
            var composed = ComposeKey(primaryKey, secondaryKey);
            if (_memoryCache.TryGetValue(composed, out T value))
                return value;
            return default;
        }

        public bool TryGetValue<T>(ServiceCacheKey primaryKey, string? secondaryKey, out T value)
        {
            var composed = ComposeKey(primaryKey, secondaryKey);
            return _memoryCache.TryGetValue(composed, out value);
        }

        public async Task<T> GetOrCreateAsync<T>(ServiceCacheKey primaryKey, string? secondaryKey, Func<Task<T>> create, CacheExpiry expiry = CacheExpiry.OneHour)
        {
            var composed = ComposeKey(primaryKey, secondaryKey);

            if (_memoryCache.TryGetValue(composed, out T existing))
                return existing;

            var result = await create();

            Set(primaryKey, secondaryKey, result, expiry);

            return result;
        }

        public void Set<T>(ServiceCacheKey primaryKey, string? secondaryKey, T value, CacheExpiry expiry = CacheExpiry.OneHour)
        {
            var composed = ComposeKey(primaryKey, secondaryKey);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDurations.GetTimespan(expiry)
            };

            _memoryCache.Set(composed, value, options);

            // record in index
            var bag = _primaryIndex.GetOrAdd(primaryKey.ToString(), _ => new ConcurrentBag<string>());
            bag.Add(composed);
        }

        public void RemovePrimary(ServiceCacheKey primaryKey)
        {
            if (_primaryIndex.TryRemove(primaryKey.ToString(), out var bag))
            {
                foreach (var key in bag)
                {
                    _memoryCache.Remove(key);
                }
            }
        }

        public void Remove(ServiceCacheKey primaryKey, string? secondaryKey = null)
        {
            if (string.IsNullOrEmpty(secondaryKey))
            {
                RemovePrimary(primaryKey);
                return;
            }

            var composed = ComposeKey(primaryKey, secondaryKey);
            _memoryCache.Remove(composed);

            // also try to remove from index bag - can't remove specific item from ConcurrentBag, so leave it (harmless)
        }

        public IEnumerable<string> GetPrimaryKeys()
        {
            return _primaryIndex.Keys;
        }

        public IEnumerable<string> GetAllKeys()
        {
            foreach (var bag in _primaryIndex.Values)
            {
                foreach (var key in bag)
                {
                    yield return key;
                }
            }
        }

        public void ClearAll()
        {
            // remove all entries from memory cache
            foreach (var bag in _primaryIndex.Values)
            {
                foreach (var key in bag)
                {
                    _memoryCache.Remove(key);
                }
            }

            _primaryIndex.Clear();
        }
    }
}
