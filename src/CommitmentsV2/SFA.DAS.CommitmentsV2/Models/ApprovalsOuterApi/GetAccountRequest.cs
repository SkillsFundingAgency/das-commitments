namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public class GetAccountRequest : IGetApiRequest
    {
        private readonly string _accountHashedId;

        public GetAccountRequest(string accountHashedId)
        {
            _accountHashedId = accountHashedId;
        }

        public string GetUrl => $"accounts/{_accountHashedId}";
    }
}