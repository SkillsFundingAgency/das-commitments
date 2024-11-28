namespace SFA.DAS.CommitmentsV2.Models;

public class ProviderAlertSummary
{
    public long ProviderId { get; set; }

    public string ProviderName { get; set; }

    public int TotalCount { get; set; }

    public int ChangesForReview { get; set; }

    public int DataMismatchCount { get; set; }
}