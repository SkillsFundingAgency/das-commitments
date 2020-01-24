using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class TransferSenderApproveCohortCommand
    {
        public long TransferRequestId { get; }
        public DateTime ApprovedOn { get; }
        public UserInfo UserInfo { get; }

        public TransferSenderApproveCohortCommand(long transferRequestId, DateTime approvedOn, UserInfo userInfo)
        {
            TransferRequestId = transferRequestId;
            ApprovedOn = approvedOn;
            UserInfo = userInfo;
        }
    }
}