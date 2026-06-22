using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types;

public enum FreezePaymentsReason : byte
{
    [Description("Learner is on a break")]
    LearnerOnBreak = 1,

    [Description("Learner has withdrawn")]
    LearnerWithdrawn = 2,

    [Description("There is a change to training details")]
    ChangeToTrainingDetails = 3,

    [Description("You disagree with an auto approved change")]
    DisagreeWithAutoApprovedChange = 4
}
