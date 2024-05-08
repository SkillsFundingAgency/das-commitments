using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters
{
    public class ApprenticeshipSearchParameters : IEmployerProviderIdentifier
    {
        public long? EmployerAccountId { get; set; }
        public long? ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public bool ReverseSort { get; set; }
        public ApprenticeshipSearchFilters Filters { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}