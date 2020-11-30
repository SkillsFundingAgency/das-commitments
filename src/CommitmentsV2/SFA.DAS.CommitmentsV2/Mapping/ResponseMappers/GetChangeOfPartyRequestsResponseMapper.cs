using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetChangeOfPartyRequestsResponseMapper : IMapper<GetChangeOfPartyRequestsQueryResult, GetChangeOfPartyRequestsResponse>
    {
        public Task<GetChangeOfPartyRequestsResponse> Map(GetChangeOfPartyRequestsQueryResult source)
        {
            return Task.FromResult(new GetChangeOfPartyRequestsResponse
            {
                ChangeOfPartyRequests = source.ChangeOfPartyRequests.Select(r =>
                    new GetChangeOfPartyRequestsResponse.ChangeOfPartyRequest
                    {
                        Id = r.Id,
                        ChangeOfPartyType = r.ChangeOfPartyType,
                        OriginatingParty = r.OriginatingParty,
                        Status = r.Status,
                        EmployerName = r.EmployerName,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        Price = r.Price,
                        CohortId = r.CohortId,
                        WithParty =  r.WithParty,
                        NewApprenticeshipId = r.NewApprenticeshipId,
                        ProviderId = r.ProviderId
                    }).ToList()
            });
        }
    }
}