using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

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
                TrainingProgrammes = result
            }; 
                
        }
    }
}