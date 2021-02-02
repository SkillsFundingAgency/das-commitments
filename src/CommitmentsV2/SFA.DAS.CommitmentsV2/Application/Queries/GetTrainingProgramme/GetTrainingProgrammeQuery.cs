using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme
{
    public class GetTrainingProgrammeQuery : IRequest<GetTrainingProgrammeQueryResult>
    {
        public string Id { get; set; }
    }
}