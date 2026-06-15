using System;

namespace SFA.DAS.Learning.Types;

public class LearningWithdrawnEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime WithdrawalDate { get; set; }
    public short WithdrawalReasonCode { get; set; }
    public DateTime Created { get; set; }
}