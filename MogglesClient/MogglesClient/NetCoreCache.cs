#if NETCORE
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public class NetCoreCache: ICache
    {
        private MemoryCache Cache { get; set; }
        private event CacheExpirationHandler CacheExpiration;

        public NetCoreCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }

        public List<FeatureToggle> GetFeatureTogglesFromCache(string cacheKey)
        {
            if (Cache.TryGetValue(cacheKey, out List<FeatureToggle> featureToggles))
            {
                return featureToggles;
            }

            return null;
        }

        public void CacheFeatureToggles(string cacheKey, List<FeatureToggle> featureToggles, DateTimeOffset? cachingTime, bool isExpiringCacheEntry = true)
        {
            Cache.Set(cacheKey, featureToggles, GetPolicy(cachingTime, isExpiringCacheEntry));
        }

        public void SubscribeToCacheExpirationEvent(CacheExpirationHandler cacheFeatureToggles)
        {
            UnsubscribeToCacheExpirationEvent(cacheFeatureToggles);
            CacheExpiration += cacheFeatureToggles;
        }


        public void UnsubscribeToCacheExpirationEvent(CacheExpirationHandler cacheFeatureToggles)
        {
            CacheExpiration -= cacheFeatureToggles;
        }

        private MemoryCacheEntryOptions GetPolicy(DateTimeOffset? cachingTime, bool isExpiringCacheEntry)
        {
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove

            };

            if (cachingTime.HasValue)
            {
                memoryCacheEntryOptions.AbsoluteExpiration = cachingTime.Value;
            }

            if (isExpiringCacheEntry)
            {
                memoryCacheEntryOptions.RegisterPostEvictionCallback(CacheRemovedCallback);
            }
            
            return memoryCacheEntryOptions;
        }

        private void CacheRemovedCallback(object key, object value,
            EvictionReason reason, object state)
        {
            if (reason != EvictionReason.Replaced && reason != EvictionReason.Removed)
            {
                OnCacheExpiration();
            }
        }

        private void OnCacheExpiration()
        {
            CacheExpiration?.Invoke();
        }
    }
}
#endif