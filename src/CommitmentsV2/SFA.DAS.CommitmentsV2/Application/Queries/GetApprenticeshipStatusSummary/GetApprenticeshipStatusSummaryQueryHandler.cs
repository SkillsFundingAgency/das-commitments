using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary
{
    public class GetApprenticeshipStatusSummaryQueryHandler : IRequestHandler<GetApprenticeshipStatusSummaryQuery, GetApprenticeshipStatusSummaryQueryResults>
    {
        private readonly IApprenticeshipStatusSummaryService _apprenticeshipStatusSummaryService;        

        public GetApprenticeshipStatusSummaryQueryHandler(IApprenticeshipStatusSummaryService apprenticeshipStatusSummaryService)
        {
            _apprenticeshipStatusSummaryService = apprenticeshipStatusSummaryService;
        }        

        public async Task<GetApprenticeshipStatusSummaryQueryResults> Handle(GetApprenticeshipStatusSummaryQuery request, CancellationToken cancellationToken)
        {           
           return await _apprenticeshipStatusSummaryService.GetApprenticeshipStatusSummary(request.EmployerAccountId, cancellationToken);          
        }
    }
}
