namespace SFA.DAS.Commitments.Domain.Entities
{
    public sealed class ProviderPaymentPriorityItem
    {
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int PriorityOrder { get; set; }
    }
}
