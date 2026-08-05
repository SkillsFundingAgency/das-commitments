using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalRulesEngine
{
    Task<CocApprovalState> DetermineApprovalState(CocApprovalDetails cocApprovalDetails);
}