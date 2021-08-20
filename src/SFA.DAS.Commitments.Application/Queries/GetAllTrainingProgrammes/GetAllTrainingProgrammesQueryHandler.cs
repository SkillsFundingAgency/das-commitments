using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
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

            var response = new List<TrainingProgramme>();
            response.AddRange(standardsResult.Result.Standards.Select(c=> new TrainingProgramme
            {
                Name = c.Title,
                CourseCode = c.LarsCode,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = ProgrammeType.Standard,
                FundingPeriods = c.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingCap = x.FundingCap
                }).ToList()
            }));
            response.AddRange(frameworksResult.Result.Frameworks.Select(c=> new TrainingProgramme
            {
                Name = c.Title,
                CourseCode = c.LarsCode,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = ProgrammeType.Framework,
                FundingPeriods = c.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingCap = x.FundingCap
                }).ToList()
            }));

            return new GetAllTrainingProgrammesQueryResponse
            {
                TrainingProgrammes = response.OrderBy(c=>c.Name).ToList()
            };
        }
    }
}