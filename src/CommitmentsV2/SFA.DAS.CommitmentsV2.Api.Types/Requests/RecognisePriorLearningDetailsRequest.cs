namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class RecognisePriorLearningDetailsRequest : SaveDataRequest
    {
        public int? ReducedPrice { get; set; }
        public int? ReducedDuration { get; set; }
    }
}
