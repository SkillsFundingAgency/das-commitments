using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    public class InMemoryCache : ICache
    {
        public Task<bool> ExistsAsync(string key)
        {
            var value = MemoryCache.Default.Get(key);

            return Task.FromResult(value != null);
        }

        public Task<T> GetCustomValueAsync<T>(string key)
        {
            return Task.FromResult((T)MemoryCache.Default.Get(key));
        }

        public Task SetCustomValueAsync<T>(string key, T customType, int secondsInCache = 300)
        {
            MemoryCache.Default.Set(key, customType, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(secondsInCache)) });

            return Task.FromResult<object>(null);
        }
    }
}