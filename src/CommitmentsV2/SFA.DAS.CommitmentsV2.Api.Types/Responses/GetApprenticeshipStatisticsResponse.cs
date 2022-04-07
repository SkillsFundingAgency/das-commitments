using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprenticeshipStatisticsResponse
    {
        public long Approved { get; set; }
        public long Paused { get; set; }
        public long Stopped { get; set; }
    }
}
