namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;

public class ValidateUlnOverlapOnStartDateQueryResult
{
    public long? HasOverlapWithApprenticeshipId { get; set; }
    public bool HasStartDateOverlap { get; set; }
}