using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class UpdateTransferApprovalForSenderRequest : SaveDataRequest
    {
        public long TransferSenderId { get; set; }
        public long TransferRequestId { get; set; }
        public long CohortId { get; set; }
        public long TransferReceiverId { get; set; }
        public TransferApprovalStatus TransferApprovalStatus { get; set; }
    }
}
