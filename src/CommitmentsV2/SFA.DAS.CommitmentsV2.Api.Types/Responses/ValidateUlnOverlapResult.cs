namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class ValidateUlnOverlapResult
{
    public string ULN { get; set; }
    public bool HasOverlappingStartDate { get; set; }
    public bool HasOverlappingEndDate { get; set; }
}