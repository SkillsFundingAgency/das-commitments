using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary
{
    public class GetDraftApprenticeshipPriorLearningSummaryQueryResult
    {
        public string CourseCode { get; set; }
        public bool? RecognisePriorLearning { get; set; }
        public int? TrainingTotalHours { get; set; }
        public int? DurationReducedByHours { get; set; }
        public bool? IsDurationReducedByRpl { get; set; }
        public int? DurationReducedBy { get; set; }
        public int? CostBeforeRpl { get; set; }
        public int? PriceReducedBy { get; set; }
        public string StandardUId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? FundingBandMaximum { get; set; }
        public decimal? PercentageOfPriorLearning { get; set; }
        public decimal? MinimumPercentageReduction { get; set; }
        public int? MinimumPriceReduction { get; set; }
        public bool RplPriceReductionError { get; set; }
    }
}