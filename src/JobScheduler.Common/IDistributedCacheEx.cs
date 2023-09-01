using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Extension methods to add strongly typed support to IDistributedCache interface
    /// </summary>
    public static class IDistributedCacheExtensions
    {
        internal static string CachePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Imlements a cache read-through method to get a value from the cache, or set it if it doesn't exists
        /// </summary>
        /// <typeparam name="T">The type of the cached object</typeparam>
        /// <param name="cache">The distributed cache</param>
        /// <param name="key">The cache key</param>
        /// <param name="factory">The method to generate the cache value if there was a cache miss</param>
        /// <param name="expiry">The expiry of the cached object</param>
        /// <returns></returns>
        public static async Task<T> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, TimeSpan? expiry)
        {
            if (factory is null) throw new ArgumentNullException(nameof(factory));
            var obj = await cache.GetAsync<T>(key);
            if (obj == null)
            {
                obj = await factory();
                await cache.SetAsync(key, obj, expiry);
            }

            return obj;
        }

        /// <summary>
        /// Get an object from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
        {
            if (cache is null) throw new ArgumentNullException(nameof(cache));
            key = CacheKey(key);
            return Deserialize<T>(await cache.GetAsync(key) ?? Array.Empty<byte>());
        }

        /// <summary>
        /// Sets an object in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T obj, TimeSpan? expiry)
        {
            if (cache is null) throw new ArgumentNullException(nameof(cache));
            key = CacheKey(key);
            await cache.SetAsync(key, Serialize(obj), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry });
        }

        /// <summary>
        /// Removes an object from the cache
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task RemoveAsync(this IDistributedCache cache, string key)
        {
            if (cache is null) throw new ArgumentNullException(nameof(cache));
            key = CacheKey(key);
            await cache.RemoveAsync(key);
        }

        private static string CacheKey(string key) => $"{CachePrefix}{key}";

        private static T? Deserialize<T>(byte[] data) => data == null || data.Length == 0 ? default(T?) : JsonSerializer.Deserialize<T?>(data);

        private static byte[] Serialize<T>(T obj) => obj == null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(obj);
    }
}