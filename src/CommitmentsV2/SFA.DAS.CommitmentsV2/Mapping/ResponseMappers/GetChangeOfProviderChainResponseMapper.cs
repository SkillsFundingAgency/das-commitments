using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetChangeOfProviderChainResponseMapper : IMapper<GetChangeOfProviderChainQueryResult, GetChangeOfProviderChainResponse>
    {
        public Task<GetChangeOfProviderChainResponse> Map(GetChangeOfProviderChainQueryResult source)
        {
            return Task.FromResult(new GetChangeOfProviderChainResponse
            {
                ChangeOfProviderChain = source.ChangeOfProviderChain.Select(r =>
                    new GetChangeOfProviderChainResponse.ChangeOfProviderLink
                    {
                        ApprenticeshipId = r.ApprenticeshipId,
                        ProviderName = r.ProviderName,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        StopDate = r.StopDate,
                        CreatedOn = r.CreatedOn
                    }).ToList()
            });
        }
    }
}