using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.RejectTransferRequest
{
    public sealed class RejectTransferRequestCommand : IAsyncRequest
    {
        public long CommitmentId { get; set; }
        public long TransferRequestId { get; set; }
        public long TransferSenderId { get; set; }
        public long TransferReceiverId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }
}
