﻿using static SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes.GetPriceEpisodesQueryResult;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class PriceExtensions
    {
        public static decimal GetPrice(this IEnumerable<PriceEpisode> priceEpisodes)
        {
            return priceEpisodes.GetPrice(DateTime.UtcNow);
        }

        private static decimal GetPrice(this IEnumerable<PriceEpisode> priceEpisodes, DateTime effectiveDate)
        {
            var episodes = priceEpisodes
                .OrderByDescending(x=> x.FromDate)
                .ToList();

            var episode = episodes.FirstOrDefault(x => 
                x.FromDate <= effectiveDate && (x.ToDate == null || x.ToDate >= effectiveDate));

            return episode?.Cost ?? episodes.First().Cost;
        }

        public static string FormatCost(this decimal? cost)
        {
            return !cost.HasValue ? string.Empty : $"£{cost.Value:n0}";
        }

        public static string FormatCost(this int? cost)
        {
            return !cost.HasValue ? string.Empty : $"£{cost.Value:n0}";
        }

        public static string FormatCost(this decimal value)
        {
            return $"£{value:n0}";
        }

        public static string FormatCost(this int value)
        {
            return $"£{value:n0}";
        }
    }
}
