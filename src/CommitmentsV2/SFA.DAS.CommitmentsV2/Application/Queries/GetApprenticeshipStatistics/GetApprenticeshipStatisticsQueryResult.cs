using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics
{
    public class GetApprenticeshipStatisticsQueryResult
    {
        public long ApprovedApprenticeshipCount { get; set; }
        public long PausedApprenticeshipCount { get; set; }
        public long StoppedApprenticeshipCount { get; set; }
    }
}
