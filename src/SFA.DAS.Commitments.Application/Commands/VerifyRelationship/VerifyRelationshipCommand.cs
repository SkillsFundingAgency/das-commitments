using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.VerifyRelationship
{
    public sealed class VerifyRelationshipCommand : IAsyncRequest
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public string LegalEntityId { get; set; }
        public bool? Verified { get; set; }
        public string UserId { get; set; }
    }
}
