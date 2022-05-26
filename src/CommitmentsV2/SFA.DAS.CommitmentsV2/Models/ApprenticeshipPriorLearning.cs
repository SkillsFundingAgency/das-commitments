using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipPriorLearning
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int? ReducedDurationBy { get; set; }
        public int? ReducedPriceBy { get; set; }
    }
}