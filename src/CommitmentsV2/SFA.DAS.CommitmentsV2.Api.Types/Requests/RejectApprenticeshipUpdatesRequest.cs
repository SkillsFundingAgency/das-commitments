namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class RejectApprenticeshipUpdatesRequest : SaveDataRequest
{
    public long ProviderId { get; set; }
    public long AccountId { get; set; }
    public long ApprenticeshipId { get; set; }
}