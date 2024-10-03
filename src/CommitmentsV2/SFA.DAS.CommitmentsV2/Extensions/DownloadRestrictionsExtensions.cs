using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DownloadRestrictionsExtensions
{
    public static IQueryable<Apprenticeship> DownloadsFilter(this IQueryable<Apprenticeship> apprenticeships,
        bool isDownload)
    {
        if (isDownload)
        {
            apprenticeships = apprenticeships
                .Where(app => !app.EndDate.HasValue || app.EndDate > DateTime.UtcNow.AddMonths(-12));
        }

        return apprenticeships;
    }
}