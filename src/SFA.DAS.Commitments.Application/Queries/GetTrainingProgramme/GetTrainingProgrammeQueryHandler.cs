using System.Threading.Tasks;
using MediatR;
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
                TrainingProgramme = result
            };
        }
    }
}