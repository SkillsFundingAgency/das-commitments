using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types;

public enum LearningSourceType : byte
{
    [Description("Approval API")]
    ApprovalAPI = 0,

    [Description("ILR status change")]
    ILRStatusChange = 1,

    [Description("Manual change")]
    ManualChange = 2,
}