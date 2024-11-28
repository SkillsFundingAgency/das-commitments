using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public partial class DataLockStatus : ITrackableEntity
{
    public long Id { get; set; }
    public long DataLockEventId { get; set; }
    public DateTime DataLockEventDatetime { get; set; }
    public string PriceEpisodeIdentifier { get; set; }
    public long ApprenticeshipId { get; set; }
    public string IlrTrainingCourseCode { get; set; }
    public DateTime? IlrActualStartDate { get; set; }
    public DateTime? IlrEffectiveFromDate { get; set; }
    public DateTime? IlrPriceEffectiveToDate { get; set; }
    public decimal? IlrTotalCost { get; set; }
    public DataLockErrorCode ErrorCode { get; set; }
    public Status Status { get; set; }
    public TriageStatus TriageStatus { get; set; }
    public long? ApprenticeshipUpdateId { get; set; }
    public bool IsResolved { get; set; }
    public EventStatus EventStatus { get; set; }
    public DateTime? Expired { get; set; }
    public bool IsExpired { get; set; }
    public TrainingType IlrTrainingType { get; set; }

    public virtual Apprenticeship Apprenticeship { get; set; }
    public virtual ApprenticeshipUpdate ApprenticeshipUpdate { get; set; }

    public void Resolve()
    {
        IsResolved = true;
    }
}

public enum TrainingType : byte
{
    Standard = 0,
    Framework = 1
}