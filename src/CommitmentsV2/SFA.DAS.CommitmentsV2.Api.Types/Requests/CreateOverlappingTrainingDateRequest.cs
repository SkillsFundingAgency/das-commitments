namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateOverlappingTrainingDateRequest: SaveDataRequest
    {
        public long ProviderId { get; set; }
        public long DraftApprenticeshipId { get; set; }
    }
}
