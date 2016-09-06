using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipCommand : IAsyncRequest
    {
        public long? ProviderId { get; set; }
        public long? AccountId { get; set; }
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }
        public Apprenticeship Apprenticeship { get; set; }
    }
}
