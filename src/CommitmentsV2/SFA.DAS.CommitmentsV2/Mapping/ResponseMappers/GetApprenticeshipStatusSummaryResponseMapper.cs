using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipStatusSummaryResponseMapper : IMapper<GetApprenticeshipStatusSummaryQueryResults, GetApprenticeshipStatusSummaryResponse>
    {
        public Task<GetApprenticeshipStatusSummaryResponse> Map(GetApprenticeshipStatusSummaryQueryResults source)
        {
            return Task.FromResult(new GetApprenticeshipStatusSummaryResponse
            {
                ApprenticeshipStatusSummaryResponse = source.GetApprenticeshipStatusSummaryQueryResult.Select(MapApprenticeship)
            });            
        }

        private ApprenticeshipStatusSummaryResponse MapApprenticeship(GetApprenticeshipStatusSummaryQueryResult source)
        {
            return new ApprenticeshipStatusSummaryResponse
            {
                ActiveCount = source.ActiveCount,
                CompletedCount = source.CompletedCount,
                LegalEntityIdentifier = source.LegalEntityIdentifier,
                LegalEntityOrganisationType = (OrganisationType) source.LegalEntityOrganisationType,
                PausedCount = source.PausedCount,
                PendingApprovalCount = source.PendingApprovalCount,
                WithdrawnCount = source.WithdrawnCount                
            };
        }
    }
}
