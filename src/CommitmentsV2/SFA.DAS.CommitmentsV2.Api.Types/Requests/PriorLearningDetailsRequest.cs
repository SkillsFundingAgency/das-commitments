namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class PriorLearningDetailsRequest : SaveDataRequest
    {
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
    }
}
