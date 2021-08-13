using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQuery : IRequest<GetTrainingProgrammeVersionResult>
    {
        public string StandardUId { get; set; }

        public GetTrainingProgrammeVersionQuery(string standardUId)
        {
            StandardUId = standardUId;
        }
    }
}
