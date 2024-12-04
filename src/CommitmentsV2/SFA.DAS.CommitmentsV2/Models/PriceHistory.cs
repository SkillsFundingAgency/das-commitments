using SFA.DAS.CommitmentsV2.Models.Interfaces;

namespace SFA.DAS.CommitmentsV2.Models;

public partial class PriceHistory :ITrackableEntity
{
    public long Id { get; set; }
    public long ApprenticeshipId { get; set; }
    public decimal Cost { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? AssessmentPrice { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public virtual Apprenticeship Apprenticeship { get; set; }
}