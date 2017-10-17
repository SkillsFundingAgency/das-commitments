using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface ICache
    {
        Task<bool> ExistsAsync(string key);
        Task<T> GetCustomValueAsync<T>(string key);
        Task SetCustomValueAsync<T>(string key, T customType, int secondsInCache = 300);
    }
}
