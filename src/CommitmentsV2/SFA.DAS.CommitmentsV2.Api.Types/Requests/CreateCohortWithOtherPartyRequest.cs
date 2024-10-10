namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class CreateCohortWithOtherPartyRequest : SaveDataRequest
{
    public long AccountId { get; set; }
    public long AccountLegalEntityId { get; set; }
    public long ProviderId { get; set; }
    public string Message { get; set; }
    public long? TransferSenderId { get; set; }
    public int? PledgeApplicationId { get; set; }
}