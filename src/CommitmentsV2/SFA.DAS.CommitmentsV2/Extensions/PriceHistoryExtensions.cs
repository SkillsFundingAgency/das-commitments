using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class PriceHistoryExtensions
    {
        public static decimal GetPrice(this IEnumerable<PriceHistory> priceEpisodes, DateTime effectiveDate)
        {
            var episode = priceEpisodes.GetCurrentEpisode(effectiveDate);
            return episode?.Cost ?? priceEpisodes.First().Cost;
        }

        public static decimal? GetTrainingPrice(this IEnumerable<PriceHistory> priceEpisodes, DateTime effectiveDate)
        {
            var episode = priceEpisodes.GetCurrentEpisode(effectiveDate);
            return episode?.TrainingPrice ?? priceEpisodes.First().TrainingPrice;
        }

        public static decimal? GetAssessmentPrice(this IEnumerable<PriceHistory> priceEpisodes, DateTime effectiveDate)
        {
            var episode = priceEpisodes.GetCurrentEpisode(effectiveDate);
            return episode?.AssessmentPrice ?? priceEpisodes.First().AssessmentPrice;
        }

        private static PriceHistory GetCurrentEpisode(this IEnumerable<PriceHistory> priceEpisodes, DateTime effectiveDate)
        {
            var episodes = priceEpisodes.ToList();
            return episodes.Find(x =>
                x.FromDate <= effectiveDate && (x.ToDate == null || x.ToDate >= effectiveDate));
        }
    }
}
