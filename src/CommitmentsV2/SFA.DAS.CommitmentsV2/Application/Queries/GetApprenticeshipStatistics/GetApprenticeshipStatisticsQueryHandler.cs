using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics
{
    public class GetApprenticeshipStatisticsQueryHandler : IRequestHandler<GetApprenticeshipStatisticsQuery, GetApprenticeshipStatisticsQueryResult>
    {
        public Task<GetApprenticeshipStatisticsQueryResult> Handle(GetApprenticeshipStatisticsQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
