namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class PriorLearningDetailsRequest : SaveDataRequest
{
    public bool Rpl2Mode { get; set; }
    public int? DurationReducedBy { get; set; }
    public int? PriceReducedBy { get; set; }
    public int? DurationReducedByHours { get; set; }
}