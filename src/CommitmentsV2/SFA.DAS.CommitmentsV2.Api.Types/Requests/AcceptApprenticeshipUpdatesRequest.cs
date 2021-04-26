namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class AcceptApprenticeshipUpdatesRequest : SaveDataRequest
    {
        //TODO: Remove providerId & AccountId if not required.
        public long ProviderId { get; set; }
        public long AccountId { get; set; }

        public long ApprenticeshipId { get; set; }
    }
}
