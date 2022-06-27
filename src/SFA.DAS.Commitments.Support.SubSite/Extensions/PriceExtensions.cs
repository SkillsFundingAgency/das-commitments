using System.Collections.Generic;
using System;
using System.Linq;
using static SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes.GetPriceEpisodesQueryResult;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class PriceExtensions
    {
        public static decimal GetPrice(this IEnumerable<PriceEpisode> priceEpisodes)
        {
            return priceEpisodes.GetPrice(DateTime.UtcNow);
        }

        public static decimal GetPrice(this IEnumerable<PriceEpisode> priceEpisodes,
            DateTime effectiveDate)
        {
            var episodes = priceEpisodes.ToList();

            var episode = episodes.FirstOrDefault(x =>
                x.FromDate <= effectiveDate && (x.ToDate == null || x.ToDate >= effectiveDate));

            return episode?.Cost ?? episodes.First().Cost;
        }

        public static string FormatCost(this decimal? cost)
        {
            if (!cost.HasValue) return string.Empty;
            return $"£{cost.Value:n0}";
        }

        public static string FormatCost(this int? cost)
        {
            if (!cost.HasValue) return string.Empty;
            return $"£{cost.Value:n0}";
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
