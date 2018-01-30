using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeships
{
    public sealed class GetApprenticeshipsRequest : IAsyncRequest<GetApprenticeshipsResponse>
    {
        public Caller Caller { get; set; }
    }
}
