namespace SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions
{
    public class GetNewerTrainingProgrammeVersionsQuery : IRequest<GetNewerTrainingProgrammeVersionsQueryResult>
    {
        public string StandardUId { get; set; }
    }
}
