using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateTransferApprovalForSender;

public class UpdateTransferApprovalForSenderCommand : IRequest
{
    public long TransferSenderId { get; }
    public long TransferReceiverId { get; }
    public long TransferRequestId { get; }
    public long CohortId { get; }
    public TransferApprovalStatus TransferApprovalStatus { get; }
    public UserInfo UserInfo { get; }
        
    public UpdateTransferApprovalForSenderCommand(long transferSenderId, long transferReceiverId, long transferRequestId, long cohortId, TransferApprovalStatus transferApprovalStatus, UserInfo userInfo)
    {
        TransferSenderId = transferSenderId;
        TransferReceiverId = transferReceiverId;
        TransferRequestId = transferRequestId;
        CohortId = cohortId;
        TransferApprovalStatus = transferApprovalStatus;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
    }
}