using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerAccountIds
{
    public sealed class GetEmployerAccountIdsQueryHandler : IAsyncRequestHandler<GetEmployerAccountIdsRequest, GetEmployerAccountIdsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public GetEmployerAccountIdsQueryHandler(IApprenticeshipRepository apprenticeshipRepository)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task<GetEmployerAccountIdsResponse> Handle(GetEmployerAccountIdsRequest message)
        {
            var apprenticeshipSummaries = await _apprenticeshipRepository.GetEmployerAccountIds();

            return new GetEmployerAccountIdsResponse
            {
                Data = apprenticeshipSummaries
            };
        }
    }
}
