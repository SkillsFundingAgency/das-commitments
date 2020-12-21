using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards
{
    public class GetAllTrainingProgrammeStandardsQueryHandler : IRequestHandler<GetAllTrainingProgrammeStandardsQuery,GetAllTrainingProgrammeStandardsQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;

        public GetAllTrainingProgrammeStandardsQueryHandler (ITrainingProgrammeLookup service)
        {
            _service = service;
        }
        public async Task<GetAllTrainingProgrammeStandardsQueryResult> Handle(GetAllTrainingProgrammeStandardsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllStandards();
            return new GetAllTrainingProgrammeStandardsQueryResult
            {
                TrainingProgrammes = result
            };
        }
    }
}