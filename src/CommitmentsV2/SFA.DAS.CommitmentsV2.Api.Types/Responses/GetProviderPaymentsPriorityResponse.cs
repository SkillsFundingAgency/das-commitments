using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetProviderPaymentsPriorityResponse
    {
        public IReadOnlyCollection<ProviderPaymentPriorityItem> ProviderPaymentPriorities { get; set; }

        public class ProviderPaymentPriorityItem
        {
            public string ProviderName { get; set; }
            public long ProviderId { get; set; }
            public int PriorityOrder { get; set; }
        }
    }
}
