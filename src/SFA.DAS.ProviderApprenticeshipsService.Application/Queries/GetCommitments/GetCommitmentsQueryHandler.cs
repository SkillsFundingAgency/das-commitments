using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments
{
    public class GetCommitmentsQueryHandler : IAsyncRequestHandler<GetCommitmentsQueryRequest, GetCommitmentsQueryResponse>
    {
        private readonly ICommitmentsApi _commitmentsApi;

        public GetCommitmentsQueryHandler(ICommitmentsApi commitmentsApi)
        {
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));
            _commitmentsApi = commitmentsApi;
        }

        public async Task<GetCommitmentsQueryResponse> Handle(GetCommitmentsQueryRequest message)
        {
            var response = await _commitmentsApi.GetProviderCommitments(message.ProviderId);

            return new GetCommitmentsQueryResponse
            {
                Commitments = response
            };
        }
    }
}