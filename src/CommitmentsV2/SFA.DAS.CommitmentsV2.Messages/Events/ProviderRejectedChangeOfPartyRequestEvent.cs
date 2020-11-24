
namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ProviderRejectedChangeOfPartyRequestEvent
    {
        public long EmployerAccountId { get; set; }
        public string TrainingProviderName { get; set; }
        public string EmployerName { get; set; }
        public string ApprenticeName { get; set; }
        public long ChangeOfPartyRequestId { get; set; }
    }
}
