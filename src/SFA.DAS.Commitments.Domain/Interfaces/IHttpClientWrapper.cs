using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IHttpClientWrapper
    {
        string AuthScheme { get; set; }
        Task<string> GetString(string url, string accessToken);
    }
}
