using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics
{
    public class GetApprenticeshipStatisticsQuery : IRequest<GetApprenticeshipStatisticsQueryResult>
    {
        public int LastNumberOfDays { get; set; }
    }
}
