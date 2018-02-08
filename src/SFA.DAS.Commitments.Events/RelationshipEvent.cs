using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("relationship_update")]
    public class RelationshipEvent
    {
        public RelationshipEvent()
        {

        }

        public RelationshipEvent(long providerId, long employerAccountId, string legalEntityId)
        {
            ProviderId = providerId;
            EmployerAccountId = employerAccountId;
            LegalEntityId = legalEntityId;
        }

        public long ProviderId { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityId { get; set; }
    }
}
