using MediatR;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsRequest : IRequest<GetApprenticeshipsResponse>
    {
        public long ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public string SortField { get; set; }
        public bool ReverseSort { get; set; }

        public ApprenticeshipSearchFilters SearchFilters { get; set; }
    }
}
