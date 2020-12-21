using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes
{
    public class GetAllTrainingProgrammesQueryHandler : IRequestHandler<GetAllTrainingProgrammesQuery, GetAllTrainingProgrammesQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;

        public GetAllTrainingProgrammesQueryHandler (ITrainingProgrammeLookup service)
        {
            _service = service;
        }
        public async Task<GetAllTrainingProgrammesQueryResult> Handle(GetAllTrainingProgrammesQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAll();
            
            return new GetAllTrainingProgrammesQueryResult
            {
                TrainingProgrammes = result.Select(c=> new TrainingProgramme
                {
                    Name = c.Name,
                    CourseCode = c.CourseCode,
                    EffectiveFrom = c.EffectiveFrom,
                    EffectiveTo = c.EffectiveTo,
                    ProgrammeType = c.ProgrammeType,
                    FundingPeriods = c.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
                    {
                        EffectiveFrom = x.EffectiveFrom,
                        EffectiveTo = x.EffectiveTo,
                        FundingCap = x.FundingCap
                    }).ToList()
                })
            }; 
                
        }
    }
}