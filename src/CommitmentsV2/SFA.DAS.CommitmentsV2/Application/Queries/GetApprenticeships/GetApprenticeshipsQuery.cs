using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsQuery : IRequest<GetApprenticeshipsQueryResult>
    {
        public long ProviderId { get; set; }
        public int PageNumber { get; set; }
        public int PageItemCount { get; set; }
        public string SortField { get; set; }
        public bool ReverseSort { get; set; }
    }
}
