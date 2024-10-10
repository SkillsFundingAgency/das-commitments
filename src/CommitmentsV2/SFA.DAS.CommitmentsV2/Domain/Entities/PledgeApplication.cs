namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class PledgeApplication
{
    public long SenderEmployerAccountId { get; set; }
    public long ReceiverEmployerAccountId { get; set; }
    public ApplicationStatus Status { get; set; }
    public bool AutomaticApproval { get; set; }
    public int TotalAmount { get; set; }
    public int AmountUsed { get; set; }
    public int AmountRemaining { get; set; }

    public enum ApplicationStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Accepted = 3,
        FundsUsed = 4,
        Declined = 5,
        Withdrawn = 6
    }
}