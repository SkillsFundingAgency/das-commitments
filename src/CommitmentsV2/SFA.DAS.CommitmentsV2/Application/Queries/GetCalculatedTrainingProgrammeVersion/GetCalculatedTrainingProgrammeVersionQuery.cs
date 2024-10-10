namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;

public class GetCalculatedTrainingProgrammeVersionQuery: IRequest<GetCalculatedTrainingProgrammeVersionQueryResult>
{
    public int CourseCode { get; set; }
    public DateTime StartDate { get; set; }
}