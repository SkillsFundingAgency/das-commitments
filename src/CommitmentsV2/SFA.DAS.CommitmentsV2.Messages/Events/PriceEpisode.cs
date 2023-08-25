using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class PriceEpisode
    {
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Cost { get; set; }
        public decimal? TrainingPrice { get; set; }
        public decimal? EndPointAssessmentPrice { get; set; }
    }
}