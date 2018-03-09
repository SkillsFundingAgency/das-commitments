using MediatR;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.TransferApproval
{
    public sealed class TransferApprovalCommand : IAsyncRequest
    {
        public long CommitmentId { get; set; }
        public long TransferSenderId { get; set; }
        public long TransferReceiverId { get; set; }
        public TransferApprovalStatus TransferApprovalStatus { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }
}
