using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public interface ICommitmentsApiClient
    {
        Task<bool> HealthCheck();

        Task<AccountLegalEntity> GetLegalEntity(long accountLegalEntityId, CancellationToken cancellationToken = default);

        // To be removed latter
        Task<string> SecureCheck();
        Task<string> SecureEmployerCheck();
        Task<string> SecureProviderCheck();
        Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request, CancellationToken cancellationToken = default);
    }
}
