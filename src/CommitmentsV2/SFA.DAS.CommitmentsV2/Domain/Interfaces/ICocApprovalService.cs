using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalService
{
    List<CocUpdateResult> DetermineCocUpdateStatuses(CocUpdates updates, Apprenticeship apprenticeship);
}