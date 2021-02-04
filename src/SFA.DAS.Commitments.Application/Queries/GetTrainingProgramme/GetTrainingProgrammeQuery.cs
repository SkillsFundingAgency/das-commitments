using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetTrainingProgramme
{
    public class GetTrainingProgrammeQuery : IAsyncRequest<GetTrainingProgrammeQueryResponse>
    {
        public string Id { get ; set ; }
    }
}