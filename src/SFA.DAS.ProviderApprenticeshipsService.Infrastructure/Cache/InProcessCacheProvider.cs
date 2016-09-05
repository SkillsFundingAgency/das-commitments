using System;
using System.Runtime.Caching;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Cache;

namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Cache
{
    public class InProcessCacheProvider : ICacheProvider
    {
        public T Get<T>(string key)
        {
            return (T)MemoryCache.Default.Get(key);
        }

        public void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            MemoryCache.Default.Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration });
        }
    }
}