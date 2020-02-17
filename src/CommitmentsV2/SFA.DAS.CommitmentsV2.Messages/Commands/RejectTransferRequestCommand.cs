using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class RejectTransferRequestCommand
    {
        public long TransferRequestId { get; }
        public DateTime RejectedOn { get; }
        public UserInfo UserInfo { get; }

        public RejectTransferRequestCommand(long transferRequestId, DateTime rejectedOn, UserInfo userInfo)
        {
            TransferRequestId = transferRequestId;
            RejectedOn = rejectedOn;
            UserInfo = userInfo;
        }
    }
}
