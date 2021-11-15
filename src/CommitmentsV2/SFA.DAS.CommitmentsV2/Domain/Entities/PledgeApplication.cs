namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class PledgeApplication
    {
        public long SenderEmployerAccountId { get; set; }
        public long ReceiverEmployerAccountId { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool AllowAutoApproval { get; set; }

        public enum ApplicationStatus : byte
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2,
            Accepted = 3,
            Declined = 4
        }
    }
}
