using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Queries.GetAllTrainingProgrammes
{
    public class GetAllTrainingProgrammesQueryHandler : IAsyncRequestHandler<GetAllTrainingProgrammesQuery, GetAllTrainingProgrammesQueryResponse>
    {
        private readonly IApprenticeshipInfoService _apprenticeshipInfoService;

        public GetAllTrainingProgrammesQueryHandler (IApprenticeshipInfoService apprenticeshipInfoService)
        {
            _apprenticeshipInfoService = apprenticeshipInfoService;
        }
        
        public async Task<GetAllTrainingProgrammesQueryResponse> Handle(GetAllTrainingProgrammesQuery message)
        {
            var standardsResult =  _apprenticeshipInfoService.GetStandards();
            var frameworksResult =  _apprenticeshipInfoService.GetFrameworks();

            await Task.WhenAll(standardsResult, frameworksResult);

            var response = new List<ITrainingProgramme>();
            response.AddRange(standardsResult.Result.Standards);
            response.AddRange(frameworksResult.Result.Frameworks);

            return new GetAllTrainingProgrammesQueryResponse
            {
                TrainingProgrammes = response.OrderBy(c=>c.Title).ToList()
            };
        }
    }
}