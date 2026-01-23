using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalService
{
    CocApprovalRequestStatus DetermineAndSetCocApprovalStatuses(CocChanges changes, Apprenticeship apprenticeship);
}