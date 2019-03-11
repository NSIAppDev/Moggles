using System;
using System.Collections.Generic;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public delegate void CacheExpirationHandler();

    public interface ICache
    {
        List<FeatureToggle> GetFeatureTogglesFromCache(string cacheKey);

        void CacheFeatureToggles(string cacheKey, List<FeatureToggle> featureToggles, DateTimeOffset? cachingTime = null, bool isExpiringCacheEntry = true);

        void SubscribeToCacheExpirationEvent(CacheExpirationHandler cacheFeatureToggles);
    }
}
