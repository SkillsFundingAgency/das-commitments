using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocManualApprovalsService
{
    bool IsFieldLockedForProvider(string fieldName, long ukPrn);
}