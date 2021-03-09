using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority
{
    public class GetProviderPaymentsPriorityQueryResult
    {
        public IReadOnlyCollection<ProviderPaymentsPriorityItem> PriorityItems { get; set; }

        public class ProviderPaymentsPriorityItem
        {
            public string ProviderName { get; set; }
            public long ProviderId { get; set; }
            public int PriorityOrder { get; set; }
        }
    }
}
