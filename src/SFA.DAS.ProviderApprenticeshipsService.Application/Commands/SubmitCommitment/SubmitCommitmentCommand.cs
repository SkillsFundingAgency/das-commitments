using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.SubmitCommitment
{
    public class SubmitCommitmentCommand : IAsyncRequest
    {
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
        public string Message { get; set; }
    }
}