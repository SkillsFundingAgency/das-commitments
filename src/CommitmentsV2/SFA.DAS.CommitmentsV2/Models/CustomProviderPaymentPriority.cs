namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class CustomProviderPaymentPriority
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public int PriorityOrder { get; set; }
    }
}
