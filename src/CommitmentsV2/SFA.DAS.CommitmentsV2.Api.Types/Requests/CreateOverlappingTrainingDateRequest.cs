namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateOverlappingTrainingDateRequest: SaveDataRequest
    {
        public long ApprenticeshipId { get; set; }
        public long PreviousApprenticeshipId { get; set; }
    }
}
