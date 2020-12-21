using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme
{
    public class GetTrainingProgrammeQueryHandler : IRequestHandler<GetTrainingProgrammeQuery, GetTrainingProgrammeQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;

        public GetTrainingProgrammeQueryHandler (ITrainingProgrammeLookup service)
        {
            _service = service;
        }
        public async Task<GetTrainingProgrammeQueryResult> Handle(GetTrainingProgrammeQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetTrainingProgramme(request.Id);
            
            return new GetTrainingProgrammeQueryResult
            {
                TrainingProgramme = new TrainingProgramme
                {
                    Name = result.Name,
                    CourseCode = result.CourseCode,
                    EffectiveFrom = result.EffectiveFrom,
                    EffectiveTo = result.EffectiveTo,
                    ProgrammeType = result.ProgrammeType,
                    FundingPeriods = result.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
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