using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes
{
    public class GetPriceEpisodesQueryResult
    {
        public IReadOnlyCollection<PriceEpisode> PriceEpisodes { get; set; }

        public class PriceEpisode
        {
            public long Id { get; set; }
            public long ApprenticeshipId { get; set; }
            public decimal Cost { get; set; }
            public decimal TrainingPrice { get; set; }
            public decimal EndPointAssessmentPrice { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
    }
}
