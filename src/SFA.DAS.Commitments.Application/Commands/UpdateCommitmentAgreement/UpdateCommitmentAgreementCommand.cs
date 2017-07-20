using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
        public string LastUpdatedByEmail { get; set; }
        public string LastUpdatedByName { get; set; }
        public LastAction LatestAction { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
