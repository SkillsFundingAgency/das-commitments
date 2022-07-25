namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class ValidateUlnOverlapOnStartDateResponse
    {
        public long? HasOverlapWithApprenticeshipId { get; set; }
        public bool HasStartDateOverlap { get; set; }
    }
}
