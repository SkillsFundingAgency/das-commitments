using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands;

public class EmployerSendCohortCommand
{
    public long CohortId { get; set; }
    public string Message { get; set; }
    public UserInfo UserInfo { get; set; }
}
