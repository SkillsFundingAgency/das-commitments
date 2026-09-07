using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;

public class ProcessApprenticeshipApprovalCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public bool ApplyChanges { get; set; }

    public UserInfo UserInfo { get; set; }
}