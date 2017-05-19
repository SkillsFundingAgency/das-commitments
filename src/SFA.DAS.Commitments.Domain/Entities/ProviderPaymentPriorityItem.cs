namespace SFA.DAS.Commitments.Domain.Entities
{
    public sealed class ProviderPaymentPriorityItem
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public int PriorityOrder { get; set; }
    }
}
