using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipPriorLearning
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
    }
}