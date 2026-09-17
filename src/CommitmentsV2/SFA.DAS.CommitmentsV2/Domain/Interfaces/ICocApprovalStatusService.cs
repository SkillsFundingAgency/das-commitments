using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalStatusService
{
    Task<List<CocUpdateResult>> DetermineCocUpdateStatusesAsync(CocApprovalDetails cocApprovalDetails);
}