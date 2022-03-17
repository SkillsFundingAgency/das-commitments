using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprovalsOuterApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
        Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request);
    }
}