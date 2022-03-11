using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Interfaces;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprovalsOuterApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
    }
}