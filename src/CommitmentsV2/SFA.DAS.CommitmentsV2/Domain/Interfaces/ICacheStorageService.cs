namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICacheStorageService
{
    Task<T> RetrieveFromCache<T>(string key);
    Task SaveToCache<T>(string key, T item, double expirationInMinutes);
    Task DeleteFromCache(string key);
}