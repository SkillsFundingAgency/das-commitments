using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class PaymentOrderChangedEvent
    {
        public long AccountId { get; set; }
        public int[] PaymentOrder { get; set; }

    }
}
