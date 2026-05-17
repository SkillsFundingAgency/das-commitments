using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types;

public enum LearningChangeType : byte
{
    [Description("Change auto approved")]
    AutoApproved = 0,

    [Description("Change rejected")]
    Rejected = 1,

    [Description("Change employer approved")]
    EmployerApproved = 2,

    [Description("Change employer rejected")]
    EmployerRejected = 3,

    [Description("Manual update")]
    ManualUpdate = 4
}