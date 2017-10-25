using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage
{
    public class ApproveDataLockTriageCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public string UserId { get; set; }
        public Caller Caller { get; set; }
    }
}
