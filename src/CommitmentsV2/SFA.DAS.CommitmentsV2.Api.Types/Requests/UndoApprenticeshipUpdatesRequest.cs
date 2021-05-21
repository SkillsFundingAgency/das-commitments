namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class UndoApprenticeshipUpdatesRequest : SaveDataRequest
    {
        public long ProviderId { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
