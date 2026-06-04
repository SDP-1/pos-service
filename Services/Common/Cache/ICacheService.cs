

namespace pos_service.Services.Common.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached value for the composed key formed by the primary and optional secondary key.
        /// </summary>
        /// <typeparam name="T">The cached value type.</typeparam>
        /// <param name="primaryKey">Primary cache key (prefix).</param>
        /// <param name="secondaryKey">Optional secondary key to compose the full cache key.</param>
        /// <returns>The cached value when present; otherwise default(T).</returns>
        T? Get<T>(ServiceCacheKey primaryKey, string? secondaryKey = null);

        /// <summary>
        /// Gets a cached value or creates it using the provided factory when missing, then caches it.
        /// </summary>
        /// <typeparam name="T">The cached value type.</typeparam>
        /// <param name="primaryKey">Primary cache key (prefix).</param>
        /// <param name="secondaryKey">Optional secondary key to compose the full cache key.</param>
        /// <param name="create">Factory function used to produce the value when missing.</param>
        /// <param name="expiry">Cache expiry policy for the created value.</param>
        /// <returns>The existing or newly created value.</returns>
        Task<T> GetOrCreateAsync<T>(ServiceCacheKey primaryKey, string? secondaryKey, Func<Task<T>> create, CacheExpiry expiry = CacheExpiry.OneHour);

        /// <summary>
        /// Sets a value in the cache for the composed key.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="primaryKey">Primary cache key (prefix).</param>
        /// <param name="secondaryKey">Optional secondary key to compose the full cache key.</param>
        /// <param name="value">Value to cache.</param>
        /// <param name="expiry">Cache expiry policy.</param>
        void Set<T>(ServiceCacheKey primaryKey, string? secondaryKey, T value, CacheExpiry expiry = CacheExpiry.OneHour);

        /// <summary>
        /// Remove all cache entries that start with the given primary key (prefix).
        /// </summary>
        /// <param name="primaryKey">Primary cache key whose group should be removed.</param>
        void RemovePrimary(ServiceCacheKey primaryKey);

        /// <summary>
        /// Remove a specific cache entry. If only primaryKey is provided, behaves same as RemovePrimary.
        /// </summary>
        /// <param name="primaryKey">Primary cache key (prefix).</param>
        /// <param name="secondaryKey">Optional secondary key to compose the full cache key.</param>
        void Remove(ServiceCacheKey primaryKey, string? secondaryKey = null);

        /// <summary>
        /// Tries to get a cached value for the composed key.
        /// </summary>
        /// <typeparam name="T">The cached value type.</typeparam>
        /// <param name="primaryKey">Primary cache key (prefix).</param>
        /// <param name="secondaryKey">Optional secondary key.</param>
        /// <param name="value">Out parameter that will receive the cached value when present.</param>
        /// <returns>True when a cached value was found; otherwise false.</returns>
        bool TryGetValue<T>(ServiceCacheKey primaryKey, string? secondaryKey, out T value);

        /// <summary>
        /// Returns the list of primary keys currently tracked by the cache index.
        /// </summary>
        System.Collections.Generic.IEnumerable<string> GetPrimaryKeys();

        /// <summary>
        /// Returns all composed cache keys currently tracked in the index.
        /// </summary>
        System.Collections.Generic.IEnumerable<string> GetAllKeys();

        /// <summary>
        /// Clears all cache entries and clears the index.
        /// </summary>
        void ClearAll();
    }
}
