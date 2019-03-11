#if NETFULL
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using MogglesClient.PublicInterface;


namespace MogglesClient
{
    public class NetFullCache : ICache
    {
        private MemoryCache Cache { get; set; }
        private event CacheExpirationHandler CacheExpiration;

        public NetFullCache()
        {
            Cache = new MemoryCache("MogglesCache");
        }

        public List<FeatureToggle> GetFeatureTogglesFromCache(string cacheKey)
        {
            if (ContainsCacheEntry(cacheKey))
            {
                return (List<FeatureToggle>)Cache[cacheKey];
            }

            return null;
        }

        public void CacheFeatureToggles(string cacheKey, List<FeatureToggle> featureToggles, DateTimeOffset? cachingTime = null, bool isExpiringCacheEntry = true)
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

        private bool ContainsCacheEntry(string cacheKey)
        {
            return Cache.Contains(cacheKey);
        }

        private CacheItemPolicy GetPolicy(DateTimeOffset? cachingTime, bool isExpiringCache)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.NotRemovable
            };

            if (cachingTime.HasValue)
            {
                cacheItemPolicy.AbsoluteExpiration = cachingTime.Value;
            }

            if (isExpiringCache)
            {
                cacheItemPolicy.RemovedCallback = CacheRemovedCallback;
            }

            return cacheItemPolicy;
        }

        private void CacheRemovedCallback(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Removed)
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
