namespace SFA.DAS.CommitmentsV2.Configuration;

public class CocApprovalsConfiguration
{
    public List<long> UkPrns { get; set; } = [];
    public List<string> LockedManualFields { get; set; } = [];
}