namespace SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;

public class DataLockStatus
{
    public long DataLockEventId { get; set; }
    public DateTime DataLockEventDatetime { get; set; }
    public string PriceEpisodeIdentifier { get; set; }
    public long ApprenticeshipId { get; set; }
    public string IlrTrainingCourseCode { get; set; }
    public TrainingType IlrTrainingType { get; set; }
    public DateTime? IlrActualStartDate { get; set; }
    public DateTime? IlrEffectiveFromDate { get; set; }
    public DateTime? IlrPriceEffectiveToDate { get; set; } //NEW
    public decimal? IlrTotalCost { get; set; }
    public Status Status { get; set; }
    public TriageStatus TriageStatus { get; set; }
    public DataLockErrorCode ErrorCode { get; set; }
    public long? ApprenticeshipUpdateId { get; set; }
    public bool IsResolved { get; set; }
    public EventStatus EventStatus { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? Expired { get; set; }
}

public enum EventStatus : byte
{
    None = 0,
    New = 1,
    Updated = 2,
    Removed = 3
}

[Flags]
public enum DataLockErrorCode
{
    /// <summary>
    /// No error
    /// </summary>
    None = 0,
    /// <summary>
    /// Error with UKPRN
    /// </summary>
    Dlock01 = 1,
    /// <summary>
    /// Error with ULN
    /// </summary>
    Dlock02 = 2,
    /// <summary>
    /// Error with Standard code
    /// </summary>
    Dlock03 = 4,
    /// <summary>
    /// Error with Framework code
    /// </summary>
    Dlock04 = 8,
    /// <summary>
    /// Error with Program type
    /// </summary>
    Dlock05 = 16,
    /// <summary>
    /// Error with Pathway code
    /// </summary>
    Dlock06 = 32,
    /// <summary>
    /// Error with Cost
    /// </summary>
    Dlock07 = 64,
    /// <summary>
    /// Error with Multiple Employers
    /// </summary>
    Dlock08 = 128,
    /// <summary>
    /// Error with Start Date too early
    /// </summary>
    Dlock09 = 256,
    /// <summary>
    /// Error with Employer stopped payments
    /// </summary>
    Dlock10 = 512
}

public enum Status : byte
{
    Unknown = 0,
    Pass = 1,
    Fail = 2
}

public enum TrainingType
{
    Standard = 0,
    Framework = 1
}

public enum TriageStatus
{
    Unknown = 0,
    Change = 1,
    Restart = 2,
    FixIlr = 3
}