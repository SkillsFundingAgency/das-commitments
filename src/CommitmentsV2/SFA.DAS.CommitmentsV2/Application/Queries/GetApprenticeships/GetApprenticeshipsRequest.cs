using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsRequest : IRequest<GetApprenticeshipsResponse>
    {
        public uint ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public string SortField { get; set; }
    }
}
