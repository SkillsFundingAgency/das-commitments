using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetDraftApprenticeshipPriorLearningSummaryResponse
    {
        public long ApprenticeshipId { get; set; }
        public long CohortId { get; set; }
        public int? TrainingTotalHours { get; set; }
        public int? DurationReducedByHours { get; set; }
        public int? CostBeforeRpl { get; set; }
        public int? PriceReducedBy { get; set; }
        public int? FundingBandMaximum { get; set; }
        public decimal? PercentageOfPriorLearning { get; set; }
        public decimal? MinimumPercentageReduction { get; set; }
        public int? MinimumPriceReduction { get; set; }
        public bool RplPriceReductionError { get; set; }
    }
}