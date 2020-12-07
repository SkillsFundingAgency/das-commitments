using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Api.Requests;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
    }
}