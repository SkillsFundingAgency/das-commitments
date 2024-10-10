namespace SFA.DAS.CommitmentsV2.Domain.Entities.DataLockProcessing;

public class DataLockUpdateResult
{
    public bool IsExpired { get; set; }
    public bool IsDuplicate { get; set; }
}