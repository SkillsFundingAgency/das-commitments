namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

public class GetApprenticeRequest : IGetApiRequest
{
    private readonly Guid _apprenticeId;

    public GetApprenticeRequest(Guid apprenticeId)
    {
        _apprenticeId = apprenticeId;
    }
    public string GetUrl => $"apprentices/{_apprenticeId}";
}