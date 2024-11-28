using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;

public class ApprenticeshipSearchResult
{
    public IEnumerable<Apprenticeship> Apprenticeships { get; set; }
    public int TotalApprenticeshipsFound { get; set; }
    public int TotalApprenticeshipsWithAlertsFound { get; set; }
    public int TotalAvailableApprenticeships { get; set; }
    public int PageNumber { get; set; }
}