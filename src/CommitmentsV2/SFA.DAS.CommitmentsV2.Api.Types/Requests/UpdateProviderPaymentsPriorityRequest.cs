using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class UpdateProviderPaymentsPriorityRequest : SaveDataRequest
{
    public List<ProviderPaymentPriorityUpdateItem> ProviderPriorities { get; set; }

    public sealed class ProviderPaymentPriorityUpdateItem
    {
        public long ProviderId { get; set; }
        public int PriorityOrder { get; set; }
    }
}