namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;

public class GetTrainingProgrammeVersionQuery : IRequest<GetTrainingProgrammeVersionQueryResult>
{
    public string StandardUId { get; set; }
    public string CourseCode { get; set; }
    public string Version { get; set; }

    public GetTrainingProgrammeVersionQuery(string standardUId)
    {
        StandardUId = standardUId;
    }

    public GetTrainingProgrammeVersionQuery(string courseCode, string version)
    {
        CourseCode = courseCode;
        Version = version;
    }
}