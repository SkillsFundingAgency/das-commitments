namespace SFA.DAS.Commitments.Domain.Entities.Validation
{
    public enum ValidationFailReason
    {
        None = 0,
        OverlappingStartDate = 1,
        OverlappingEndDate = 2,
        DateEmbrace = 3,
        DateWithin = 4
    }
}
