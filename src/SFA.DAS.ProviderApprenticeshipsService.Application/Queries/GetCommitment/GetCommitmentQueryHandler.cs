using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitment
{
    public class GetCommitmentQueryHandler : IAsyncRequestHandler<GetCommitmentQueryRequest, GetCommitmentQueryResponse>
    {
        private readonly ICommitmentsApi _commitmentsApi;

        public GetCommitmentQueryHandler(ICommitmentsApi commitmentsApi)
        {
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));
            _commitmentsApi = commitmentsApi;
        }

        public async Task<GetCommitmentQueryResponse> Handle(GetCommitmentQueryRequest message)
        {
            var commitment = await _commitmentsApi.GetProviderCommitment(message.ProviderId, message.CommitmentId);

            return new GetCommitmentQueryResponse
            {
                Commitment = commitment
            };
        }
    }
}