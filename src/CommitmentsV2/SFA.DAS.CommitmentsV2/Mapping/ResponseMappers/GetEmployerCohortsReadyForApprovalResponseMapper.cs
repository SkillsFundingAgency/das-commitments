using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetEmployerCohortsReadyForApprovalResponseMapper : IMapper<GetEmployerCohortsReadyForApprovalQueryResults, GetEmployerCohortsReadyForApprovalResponse>
    {
        public Task<GetEmployerCohortsReadyForApprovalResponse> Map(GetEmployerCohortsReadyForApprovalQueryResults source)
        {
            return Task.FromResult(new GetEmployerCohortsReadyForApprovalResponse
            {
                EmployerCohortsReadyForApprovalResponse = source.GetEmployerCohortsReadyForApprovalQueryResult.Select(MapApprenticeship)
            });
        }

        private EmployerCohortsReadyForApprovalResponse MapApprenticeship(GetEmployerCohortsReadyForApprovalQueryResult source)
        {
            return new EmployerCohortsReadyForApprovalResponse
            {
                CohortId = source.CohortId,
                CohortReference = source.CohortReference,
                AccountLegalEntityPublicHashedId = source.AccountLegalEntityPublicHashedId,
                AccountId = source.AccountId,
                AccountLegalEntityId = source.AccountLegalEntityId,
                LegalEntityName = source.LegalEntityName,
                ProviderId = source.ProviderId,
                TransferSenderId = source.TransferSenderId,
                TransferSenderName = source.TransferSenderName
            };
        }
    }
}