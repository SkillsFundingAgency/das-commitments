using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    // Note: Have currently broken the CQRS pattern here as need to return the Id.
    public sealed class CreateCommitmentCommand : IAsyncRequest<long>
    {
        public Commitment Commitment { get; set; }

        public string UserId { get; set; }
    }
}
