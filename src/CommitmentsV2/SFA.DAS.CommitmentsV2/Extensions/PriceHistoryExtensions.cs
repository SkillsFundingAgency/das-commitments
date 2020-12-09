using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class PriceHistoryExtensions
    {
        public static decimal GetPrice(this IEnumerable<PriceHistory> priceEpisodes, DateTime effectiveDate)
        {
            var episodes = priceEpisodes.ToList();
            var episode = episodes.FirstOrDefault(x =>
                x.FromDate <= effectiveDate && (x.ToDate == null || x.ToDate >= effectiveDate));
            return episode?.Cost ?? episodes.First().Cost;
        }
    }
}
