using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics
{
    public class GetApprenticeshipStatisticsQueryHandler : IRequestHandler<GetApprenticeshipStatisticsQuery, GetApprenticeshipStatisticsQueryResult>
    {
        private readonly IApprenticeshipStatusSummaryService _apprenticeshipStatusSummaryService;

        public GetApprenticeshipStatisticsQueryHandler(IApprenticeshipStatusSummaryService apprenticeshipStatusSummaryService)
        {
            _apprenticeshipStatusSummaryService = apprenticeshipStatusSummaryService;
        }

        public async Task<GetApprenticeshipStatisticsQueryResult> Handle(GetApprenticeshipStatisticsQuery request, CancellationToken cancellationToken)
        {
            return await _apprenticeshipStatusSummaryService.GetApprenticeshipStatisticsFor(request.LastNumberOfDays);
        }
    }
}
