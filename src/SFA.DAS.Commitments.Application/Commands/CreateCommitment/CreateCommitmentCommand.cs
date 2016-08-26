using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommand : IAsyncRequest
    {
        public Commitment Commitment { get; set; }
    }
}
