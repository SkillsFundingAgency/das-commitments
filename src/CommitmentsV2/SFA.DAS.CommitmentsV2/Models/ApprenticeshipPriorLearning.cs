using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipPriorLearning
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
        public double? DurationReducedByHours { get; set; }
        public double? WeightageReducedBy { get; set; }
        public string Qualification { get; set; }
        public string Reason { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}