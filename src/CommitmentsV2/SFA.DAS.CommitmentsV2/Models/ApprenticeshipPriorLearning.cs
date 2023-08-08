using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipPriorLearning
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
        public int? DurationReducedByHours { get; set; }
        public int? WeightageReducedBy { get; set; }
        public string QualificationsForRplReduction { get; set; }
        public string ReasonForRplReduction { get; set; }
        public bool? IsDurationReducedByRpl { get; set; }
    }
}