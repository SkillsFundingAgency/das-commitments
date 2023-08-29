namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ChangePriceRequest : SaveDataRequest
    {
        public int? TrainingPrice { get; set; }
        public int? EndPointAssessmentPrice { get; set; }
        public string ChangePriceReason { get; set; }
    }
}
