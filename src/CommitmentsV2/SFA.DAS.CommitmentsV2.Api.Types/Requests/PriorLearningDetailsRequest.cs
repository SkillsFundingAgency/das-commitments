namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class PriorLearningDetailsRequest : SaveDataRequest
    {
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
        public double? DurationReducedByHours { get; set; }
        public double? WeightageReducedBy { get; set; }
        public string Qualification { get; set; }
        public string Reason { get; set; }
    }
}
