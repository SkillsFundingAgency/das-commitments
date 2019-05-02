using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class PaymentOrderChangedEvent
    {
        public long EmployerAccountId { get; set; }
        public ProviderPaymentOrder[] ProviderPaymentOrder { get; set; }

    }
}
