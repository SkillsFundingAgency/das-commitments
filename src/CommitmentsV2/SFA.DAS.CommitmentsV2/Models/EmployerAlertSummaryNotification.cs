namespace SFA.DAS.CommitmentsV2.Models;

public class EmployerAlertSummaryNotification
{
    public string EmployerHashedAccountId { get; set; }
    public int TotalCount { get; set; }
    public int ChangesForReviewCount { get; set; }
    public int RestartRequestCount { get; set; }
    public int RequestsForReviewCount { get; set; }
}