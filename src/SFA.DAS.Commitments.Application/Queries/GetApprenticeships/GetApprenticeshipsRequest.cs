using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeships
{
    public sealed class GetApprenticeshipsRequest : IAsyncRequest<GetApprenticeshipsResponse>
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 25;

        public Caller Caller { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
