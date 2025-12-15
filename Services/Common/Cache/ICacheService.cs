using System;
using System.Threading.Tasks;

namespace pos_service.Services.Common.Cache
{
    public interface ICacheService
    {
        T? Get<T>(ServiceCacheKey primaryKey, string? secondaryKey = null);

        Task<T> GetOrCreateAsync<T>(ServiceCacheKey primaryKey, string? secondaryKey, Func<Task<T>> create, CacheExpiry expiry = CacheExpiry.OneHour);

        void Set<T>(ServiceCacheKey primaryKey, string? secondaryKey, T value, CacheExpiry expiry = CacheExpiry.OneHour);

        /// <summary>
        /// Remove all cache entries that start with the given primary key (prefix).
        /// </summary>
        void RemovePrimary(ServiceCacheKey primaryKey);

        /// <summary>
        /// Remove a specific cache entry. If only primaryKey is provided, behaves same as RemovePrimary.
        /// </summary>
        void Remove(ServiceCacheKey primaryKey, string? secondaryKey = null);

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
