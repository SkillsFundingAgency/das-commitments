using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
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
                TrainingProgrammes = result.Standards.Select(c=> new TrainingProgramme
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
                }).OrderBy(c=>c.Name)
            };
        }
    }
}