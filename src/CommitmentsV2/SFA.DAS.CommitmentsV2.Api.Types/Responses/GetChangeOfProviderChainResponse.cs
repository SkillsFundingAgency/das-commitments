﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetChangeOfProviderChainResponse
    {
        public IReadOnlyCollection<ChangeOfProviderLink> ChangeOfProviderChain { get; set; }

        public class ChangeOfProviderLink
        {
            public long ApprenticeshipId { get; set; }
            public string ProviderName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
    }
}
