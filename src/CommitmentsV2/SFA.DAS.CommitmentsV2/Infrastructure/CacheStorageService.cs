using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure;

public class CacheStorageService(IDistributedCache distributedCache) : ICacheStorageService
{
    public async Task SaveToCache<T>(string key, T item, double expirationInMinutes)
    {
        var json = JsonConvert.SerializeObject(item);

        await distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationInMinutes)
        });
    }

    public async Task<T> RetrieveFromCache<T>(string key)
    {
        var json = await distributedCache.GetStringAsync(key);
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }

    public async Task DeleteFromCache(string key)
    {
        await distributedCache.RemoveAsync(key);
    }
}