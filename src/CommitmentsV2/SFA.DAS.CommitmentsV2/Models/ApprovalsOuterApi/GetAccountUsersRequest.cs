namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

public class GetAccountUsersRequest : IGetApiRequest
{
    private readonly string _accountHashedId;

    public GetAccountUsersRequest(string accountHashedId)
    {
        _accountHashedId = accountHashedId;
    }

    public string GetUrl => $"/accounts/{_accountHashedId}/users";
}