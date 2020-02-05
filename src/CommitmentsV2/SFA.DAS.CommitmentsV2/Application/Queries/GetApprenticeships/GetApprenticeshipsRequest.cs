using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsRequest : IRequest<GetApprenticeshipsResponse>
    {
        public uint ProviderId { get; set; }
        public string SortField { get; set; }
        public bool ReverseSort { get; set; }
        public bool IsDownload { get; set; }
    }
}
