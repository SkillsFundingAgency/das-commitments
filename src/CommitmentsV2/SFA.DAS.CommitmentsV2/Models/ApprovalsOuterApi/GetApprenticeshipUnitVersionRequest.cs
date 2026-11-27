namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

public class GetApprenticeshipUnitVersionRequest(string courseCode, DateTime startDate) : IGetApiRequest
{
    public string GetUrl => FormattableString.Invariant(
        $"trainingcourses/apprenticeship-units/{Uri.EscapeDataString(courseCode)}/version?startDate={startDate:yyyy-MM-dd}");
}
