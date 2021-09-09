using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class GetTrainingProgrammeOverallStartAndEndDatesQueryHandler: IRequestHandler<GetTrainingProgrammeOverallStartAndEndDatesQuery, GetTrainingProgrammeOverallStartAndEndDatesQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;

        public GetTrainingProgrammeOverallStartAndEndDatesQueryHandler(ITrainingProgrammeLookup service)
        {
            _service = service;
        }
        public async Task<GetTrainingProgrammeOverallStartAndEndDatesQueryResult> Handle(GetTrainingProgrammeOverallStartAndEndDatesQuery request, CancellationToken cancellationToken)
        {
            var (effectiveFrom, effectiveTo) = await _service.GetTrainingProgrammeOverallStartAndEndDates(request.CourseCode);

            return new GetTrainingProgrammeOverallStartAndEndDatesQueryResult
            {
                TrainingProgrammeEffectiveFrom = effectiveFrom,
                TrainingProgrammeEffectiveTo = effectiveTo
            };
        }
    }
}
