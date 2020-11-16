
namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ChangeOfProviderRequestCreatedEvent
    {

        public ChangeOfProviderRequestCreatedEvent(string apprenticeName, string employerName, long providerId, string cohortReference)
        {
            ApprenticeName = apprenticeName;
            EmployerName = employerName;
            ProviderId = providerId;
            CohortReference = cohortReference;
        }

        public string ApprenticeName { get; set; }
        public string EmployerName { get; set; }
        public long ProviderId { get; set; }
        public string CohortReference { get; set; }
    }
}
