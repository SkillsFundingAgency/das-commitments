namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateCohortWithOtherPartyRequest : SaveDataRequest
    {
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }
    }
}
