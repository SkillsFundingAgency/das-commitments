using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
using SFA.DAS.Commitments.Application.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetTrainingProgramme
{
    public class GetTrainingProgrammeQueryHandler : IAsyncRequestHandler<GetTrainingProgrammeQuery, GetTrainingProgrammeQueryResponse>
    {
        private readonly IApprenticeshipInfoService _apprenticeshipInfoService;

        public GetTrainingProgrammeQueryHandler (IApprenticeshipInfoService apprenticeshipInfoService)
        {
            _apprenticeshipInfoService = apprenticeshipInfoService;
        }
        
        public async Task<GetTrainingProgrammeQueryResponse> Handle(GetTrainingProgrammeQuery message)
        {
            var result = await _apprenticeshipInfoService.GetTrainingProgram(message.Id);
            
            return new GetTrainingProgrammeQueryResponse
            {
                TrainingProgramme = new TrainingProgramme
                {
                    Name = result.Title,
                    CourseCode = result.Id,
                    EffectiveFrom = result.EffectiveFrom,
                    EffectiveTo = result.EffectiveTo,
                    ProgrammeType = int.TryParse(result.Id, out var code) ?  ProgrammeType.Standard : ProgrammeType.Framework,
                    FundingPeriods = result.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
                    {
                        EffectiveFrom = x.EffectiveFrom,
                        EffectiveTo = x.EffectiveTo,
                        FundingCap = x.FundingCap
                    }).ToList()
                }
            };
        }
    }
}