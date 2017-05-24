namespace SFA.DAS.Commitments.Domain.Entities
{
    public sealed class ProviderPaymentPriorityUpdateItem
    {
        public long ProviderId { get; set; }
        public int PriorityOrder { get; set; }
    }
}
