namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public class GetPledgeApplicationRequest : IGetApiRequest
    {
    private readonly int _pledgeApplicationId;

    public GetPledgeApplicationRequest(int pledgeApplicationId)
    {
        _pledgeApplicationId = pledgeApplicationId;
    }
    public string GetUrl => $"pledgeapplications/{_pledgeApplicationId}";
    }
}
