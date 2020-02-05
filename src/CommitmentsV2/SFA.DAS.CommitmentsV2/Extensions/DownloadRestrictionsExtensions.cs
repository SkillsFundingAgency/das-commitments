using System;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class DownloadRestrictionsExtensions
    {
        public static IQueryable<Apprenticeship> DownloadsFilter(this IQueryable<Apprenticeship> apprenticeships,
            int pageNumber)
        {
            if (pageNumber == 0)
            {
                apprenticeships = apprenticeships.Where(app => app.EndDate > DateTime.UtcNow.AddMonths(-12));
            }

            return apprenticeships;
        }
    }
}