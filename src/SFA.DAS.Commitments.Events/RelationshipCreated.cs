using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("relationship_created")]
    public class RelationshipCreated
    {
        public RelationshipCreated()
        {

        }

        public RelationshipCreated(Relationship relationship)
        {
            Relationship = relationship;
        }

        public Relationship Relationship { get; set; }

    }
}
