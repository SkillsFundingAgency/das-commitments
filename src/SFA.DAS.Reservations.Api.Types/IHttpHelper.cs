using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IHttpHelper
    {
        Task<T> GetAsync<T>(string url, object data);
    }
}