using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetChangeOfEmployerChainResponseMapper : IMapper<GetChangeOfEmployerChainQueryResult, GetChangeOfEmployerChainResponse>
    {
        public Task<GetChangeOfEmployerChainResponse> Map(GetChangeOfEmployerChainQueryResult source)
        {
            return Task.FromResult(new GetChangeOfEmployerChainResponse
            {
                ChangeOfEmployerChain = source.ChangeOfEmployerChain.Select(r =>
                    new GetChangeOfEmployerChainResponse.ChangeOfEmployerLink
                    {
                        ApprenticeshipId = r.ApprenticeshipId,
                        EmployerName = r.EmployerName,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        StopDate = r.StopDate,
                        CreatedOn = r.CreatedOn
                    }).ToList()
            });
        }
    }
}