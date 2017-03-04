using MediatR;
using SFA.DAS.Commitments.Api.Types;


namespace SFA.DAS.Commitments.Application.Commands.CreateRelationship
{
    public sealed class CreateRelationshipCommand: IAsyncRequest
    {
        public Relationship Relationship { get; set; }
    }
}
