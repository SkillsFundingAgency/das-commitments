using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions
{
    public class GetTrainingProgrammeVersionsQuery : IRequest<GetTrainingProgrammeVersionsQueryResult>
    {
        public string Id { get; set; }

        public GetTrainingProgrammeVersionsQuery(string id)
        {
            Id = id;
        }
    }
}
