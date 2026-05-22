using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalRulesEngine
{
    CocApprovalState DetermineApprovalState(CocApprovalDetails cocApprovalDetails);
}