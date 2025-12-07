namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class DraftApprenticeshipAddEmailRequest
{
    public string Email { get; set; }
    public long CohortId {  get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}
