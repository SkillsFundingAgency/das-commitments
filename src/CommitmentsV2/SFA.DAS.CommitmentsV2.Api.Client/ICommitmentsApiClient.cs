using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public interface ICommitmentsApiClient
    {
        Task<bool> HealthCheck();
        Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request);
    }
}
