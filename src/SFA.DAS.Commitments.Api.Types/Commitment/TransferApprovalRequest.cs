namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public sealed class TransferApprovalRequest
    {
        public long TransferReceiverId { get; set; }
        public TransferApprovalStatus TransferApprovalStatus { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }

    }
}
