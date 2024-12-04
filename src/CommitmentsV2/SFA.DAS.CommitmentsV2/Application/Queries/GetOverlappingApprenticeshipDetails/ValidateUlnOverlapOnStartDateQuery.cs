namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;

public class ValidateUlnOverlapOnStartDateQuery : IRequest<ValidateUlnOverlapOnStartDateQueryResult>
{
    public long DraftApprenticeshipId { get; set; }
    public long ProviderId { get; set; }
    public string Uln { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}