namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class RecognisePriorLearningDetailsRequest : SaveDataRequest
    {
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
    }
}
