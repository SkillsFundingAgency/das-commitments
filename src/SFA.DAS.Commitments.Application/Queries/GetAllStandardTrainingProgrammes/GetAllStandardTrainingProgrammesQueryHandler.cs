using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Application.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes
{
    public class GetAllStandardTrainingProgrammesQueryHandler : IAsyncRequestHandler<GetAllStandardTrainingProgrammesQuery,GetAllStandardTrainingProgrammesQueryResponse>
    {
        private readonly IApprenticeshipInfoService _apprenticeshipInfoService;

        public GetAllStandardTrainingProgrammesQueryHandler (IApprenticeshipInfoService apprenticeshipInfoService)
        {
            _apprenticeshipInfoService = apprenticeshipInfoService;
        }
        public async Task<GetAllStandardTrainingProgrammesQueryResponse> Handle(GetAllStandardTrainingProgrammesQuery message)
        {
            var result = await _apprenticeshipInfoService.GetStandards();
            
            return new GetAllStandardTrainingProgrammesQueryResponse
            {
                TrainingProgrammes = result.Standards.OrderBy(c=>c.Title).ToList()
            };
        }
    }
}