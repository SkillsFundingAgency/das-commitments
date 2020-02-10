using System.Threading;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Parameters
{
    public class ApprenticeshipSearchParameters
    {
        public long? ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public bool ReverseSort { get; set; }
        public ApprenticeshipSearchFilters Filters { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}