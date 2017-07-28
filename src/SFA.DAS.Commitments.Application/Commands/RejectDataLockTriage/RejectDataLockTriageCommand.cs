using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage
{
    public class RejectDataLockTriageCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public string UserId { get; set; }

    }
}
