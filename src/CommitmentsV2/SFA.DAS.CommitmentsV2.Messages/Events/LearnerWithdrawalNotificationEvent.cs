namespace SFA.DAS.CommitmentsV2.Messages.Events;

public class LearnerWithdrawalNotificationEvent
{
    public long ApprenticeshipId { get; set; }
    public bool IsWithdrawalFromIlr { get; set; }
}