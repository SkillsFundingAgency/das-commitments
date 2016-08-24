using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments
{
    public class GetCommitmentsQueryHandler : IAsyncRequestHandler<GetCommitmentsQueryRequest, GetCommitmentsQueryResponse>
    {
        public Task<GetCommitmentsQueryResponse> Handle(GetCommitmentsQueryRequest message)
        {
            throw new System.NotImplementedException();
        }
    }
}