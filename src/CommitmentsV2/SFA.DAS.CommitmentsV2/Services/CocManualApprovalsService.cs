using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
namespace SFA.DAS.CommitmentsV2.Services;

public class CocManualApprovalsService(CocApprovalsConfiguration cocApprovalsConfiguration) : ICocManualApprovalsService
{
    public bool IsFieldLockedForProvider(string fieldName, long ukPrn)
    {
        if(cocApprovalsConfiguration.UkPrns == null)
        {
            return false;
        }
        if (!cocApprovalsConfiguration.UkPrns.Contains(ukPrn))
        {
            return false;
        }
        if (cocApprovalsConfiguration.LockedManualFields == null)
        {
            return false;
        }
        return cocApprovalsConfiguration.LockedManualFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
    }
}