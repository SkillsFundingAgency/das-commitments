namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipPriorLearning
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
        public int? DurationReducedByHours { get; set; }
        public bool? IsDurationReducedByRpl { get; set; }
    }
}