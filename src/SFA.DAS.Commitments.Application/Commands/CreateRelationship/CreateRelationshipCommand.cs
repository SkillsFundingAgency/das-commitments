using MediatR;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.CreateRelationship
{
    public sealed class CreateRelationshipCommand: IAsyncRequest
    {
        public Relationship Relationship { get; set; }
    }
}
