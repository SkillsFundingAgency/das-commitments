using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class TransferRequestApprovedEvent
    {
        public long TransferRequestId { get; }
        public long CohortId { get; }
        public int? PledgeApplicationId { get; }
        public int NumberOfApprentices { get; }
        public decimal? FundingCap { get; }
        public DateTime ApprovedOn { get; }
        public UserInfo UserInfo { get; }

        public TransferRequestApprovedEvent(long transferRequestId, long cohortId, DateTime approvedOn, UserInfo userInfo, int numberOfApprentices, decimal? fundingCap, int? pledgeApplicationId=null)
        {
            TransferRequestId = transferRequestId;
            CohortId = cohortId;
            ApprovedOn = approvedOn;
            UserInfo = userInfo;
            NumberOfApprentices = numberOfApprentices;
            FundingCap = fundingCap;
            PledgeApplicationId = pledgeApplicationId;
        }
    }
}