namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocManualApprovalsService
{
    bool IsFieldLockedForProvider(string fieldName, long ukPrn);
}