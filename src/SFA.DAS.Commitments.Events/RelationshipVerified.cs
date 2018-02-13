using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("relationship_verified")]
    public class RelationshipVerified
    {
        public RelationshipVerified()
        {
            
        }

        public RelationshipVerified(long providerId, long employerAccountId, string legalEntityId, bool? verified)
        {
            ProviderId = providerId;
            EmployerAccountId = employerAccountId;
            LegalEntityId = legalEntityId;
            Verified = verified;
        }

        public long ProviderId { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityId { get; set; }
        public bool? Verified { get; set; }
    }
}
