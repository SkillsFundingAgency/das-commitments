using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprovalRequestAlertSeen;

public class UpdateApprovalRequestAlertAcknowledgeCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public long AccountId { get; set; }
    public List<UpdateApprovalRequestAlertAcknowledge> ApprovalRequests { get; set; }
}

public class UpdateApprovalRequestAlertAcknowledge
{
    public Guid ApprovalRequestId { get; set; }
    public bool Acknowledged { get; set; }
    public UserInfo UserInfo { get; set; }
}