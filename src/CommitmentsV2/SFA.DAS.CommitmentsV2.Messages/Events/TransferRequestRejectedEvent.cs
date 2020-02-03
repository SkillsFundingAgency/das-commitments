using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class TransferRequestRejectedEvent
    {
        public long TransferRequestId { get; }
        public long CohortId { get; }
        public DateTime RejectedOn { get; }
        public UserInfo UserInfo { get; }

        public TransferRequestRejectedEvent(long transferRequestId, long cohortId, DateTime rejectedOn, UserInfo userInfo)
        {
            TransferRequestId = transferRequestId;
            CohortId = cohortId;
            RejectedOn = rejectedOn;
            UserInfo = userInfo;
        }
    }
}