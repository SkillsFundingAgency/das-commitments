using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationship
{
    public class GetRelationshipRequest : IAsyncRequest<GetRelationshipResponse>
    {
        public Caller Caller { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public string LegalEntityId { get; set; }

    }
}
