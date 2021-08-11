using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryHandler : IRequestHandler<GetTrainingProgrammeVersionQuery, GetTrainingProgrammeVersionQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;

        public GetTrainingProgrammeVersionQueryHandler(ITrainingProgrammeLookup service)
        {
            _service = service;
        }
        public async Task<GetTrainingProgrammeVersionQueryResult> Handle(GetTrainingProgrammeVersionQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetTrainingProgrammeVersion(request.CourseCode, request.StartDate);

            if (result == null)
            {
                return new GetTrainingProgrammeVersionQueryResult
                {
                    TrainingProgramme = null
                };
            }

            return new GetTrainingProgrammeVersionQueryResult
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
