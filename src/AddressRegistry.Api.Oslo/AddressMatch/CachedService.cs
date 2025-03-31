namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System;
    using System.Threading;
    using Microsoft.Extensions.Caching.Memory;

    public abstract class CachedService
    {
        private readonly IMemoryCache _cache;

        protected CachedService(IMemoryCache memoryCache)
            => _cache = memoryCache;

        protected T2? GetOrAdd<T, T2>(
            string key,
            Func<T?> getter,
            TimeSpan cacheDuration,
            Func<T?, T2?> ifCacheHit,
            Func<T2?> ifCacheNotAvailable,
            object cacheLock)
            where T : class
        {
            if (_cache.Get(key) is T cached)
                return ifCacheHit(cached);

            if (Monitor.IsEntered(cacheLock))
                return ifCacheNotAvailable();

            lock (cacheLock)
            {
                if (_cache.Get(key) is T cachedLock)
                    return ifCacheHit(cachedLock);

                T? item = getter();
                if (item is not null)
                    _cache.Set(
                        key,
                        item,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = new DateTimeOffset(DateTime.Now.Add(cacheDuration))
                        });

                return ifCacheHit(item);
            }
        }

        protected T? GetOrAdd<T>(
            string key,
            Func<T?> getter,
            TimeSpan cacheDuration,
            object cacheLock)
            where T : class
        {
            if (_cache.Get(key) is T cached)
                return cached;

            lock (cacheLock)
            {
                if (_cache.Get(key) is T lockedCache)
                    return lockedCache;

                T? item = getter();
                if (item is not null)
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
