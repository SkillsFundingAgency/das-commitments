using System;

namespace SFA.DAS.Learning.Types.Models
{
    public class ApprenticeshipEpisodePrice
    {
        public Guid Key { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? TrainingPrice { get; set; }
        public decimal? EndPointAssessmentPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int FundingBandMaximum { get; set; }
    }
}