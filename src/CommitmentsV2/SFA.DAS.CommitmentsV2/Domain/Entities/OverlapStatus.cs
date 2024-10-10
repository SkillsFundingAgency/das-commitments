namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public enum OverlapStatus : short
{
    None = 0,
    OverlappingStartDate = 1,
    OverlappingEndDate = 2,
    DateEmbrace = 3,
    DateWithin = 4
}