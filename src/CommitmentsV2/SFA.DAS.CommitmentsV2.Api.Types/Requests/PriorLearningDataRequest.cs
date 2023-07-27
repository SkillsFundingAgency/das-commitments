namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class PriorLearningDataRequest : SaveDataRequest
    {
        public int? TrainingTotalHours { get; set; }
        public int? DurationReducedByHours { get; set; }
        public bool? IsDurationReducedByRpl { get; set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
    }
}
