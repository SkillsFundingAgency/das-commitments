using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalStatusService
{
    List<CocUpdateResult> DetermineCocUpdateStatuses(CocApprovalDetails cocApprovalDetails);
}