namespace AddressRegistry.Api.Legacy.AddressMatch.V1.Matching
{
    using System;
    using Microsoft.Extensions.Caching.Memory;

    public abstract class CachedService
    {
        private static readonly object CacheLock = new object();
        private readonly IMemoryCache _cache;

        protected CachedService(IMemoryCache memoryCache)
            => _cache = memoryCache;

        protected T2 GetOrAdd<T, T2>(
            string key,
            Func<T> getter,
            TimeSpan cacheDuration,
            Func<T, T2> ifCacheHit,
            Func<T2> ifCacheNotHit)
            where T : class
        {
            lock (CacheLock)
            {
                if (_cache.Get(key) is T cached)
                    return ifCacheHit(cached);

                cached = _cache.Get(key) as T;
                if (cached != null)
                    return ifCacheNotHit();

                T item = getter();
                if (item != null)
                    _cache.Set(
                        key,
                        item,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = new DateTimeOffset(DateTime.Now.Add(cacheDuration))
                        });
            }

            return ifCacheNotHit();
        }

        protected T GetOrAdd<T>(
            string key,
            Func<T> getter,
            TimeSpan cacheDuration)
            where T : class
        {
            lock (CacheLock)
            {
                if (_cache.Get(key) is T cached)
                    return cached;

                cached = _cache.Get(key) as T;
                if (cached != null)
                    return cached;

                T item = getter();
                if (item != null)
                    _cache.Set(
                        key,
                        item,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = new DateTimeOffset(DateTime.Now.Add(cacheDuration))
                        });

                return item;
            }
        }
    }
}
