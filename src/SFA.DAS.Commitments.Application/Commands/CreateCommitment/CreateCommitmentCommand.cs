using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    // Note: Have currently broken the CQRS pattern here as need to return the Id.
    public sealed class CreateCommitmentCommand : IAsyncRequest<long>
    {
        public Commitment Commitment { get; set; }

        public string UserId { get; set; }

        public CallerType CallerType { get; set; }
    }
}
