using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class ApproveTransferRequestCommand
    {
        public long TransferRequestId { get; }
        public DateTime ApprovedOn { get; }
        public UserInfo UserInfo { get; }

        public ApproveTransferRequestCommand(long transferRequestId, DateTime approvedOn, UserInfo userInfo)
        {
            TransferRequestId = transferRequestId;
            ApprovedOn = approvedOn;
            UserInfo = userInfo;
        }
    }
}