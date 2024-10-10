namespace SFA.DAS.CommitmentsV2.Models;

public class DataLockUpdaterJobHistory
{
    public long Id { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime StartedOn { get; set; }
    public DateTime FinishedOn { get; set; }
    public long FromEventId { get; set; }
    public int ItemCount { get; set; }
    public int SkippedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ExpiredCount { get; set; }
    public int PagesRemaining { get; set; }

    public DataLockUpdaterJobHistory()
    {
        CorrelationId = Guid.NewGuid();
        StartedOn = DateTime.UtcNow;
    }
}