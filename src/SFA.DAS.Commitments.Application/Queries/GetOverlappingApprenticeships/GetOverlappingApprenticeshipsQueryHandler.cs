using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public sealed class GetOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetOverlappingApprenticeshipsRequest, GetOverlappingApprenticeshipsResponse>
    {
        public Task<GetOverlappingApprenticeshipsResponse> Handle(GetOverlappingApprenticeshipsRequest query)
        {
            throw new System.NotImplementedException();
        }
    }
}
