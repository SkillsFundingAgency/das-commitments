using System;

namespace SFA.DAS.ProviderApprenticeshipsService.Domain.Cache
{
    public interface ICacheProvider
    {
        T Get<T>(string key);
        void Set(string key, object value, DateTimeOffset absoluteExpiration);
    }
}