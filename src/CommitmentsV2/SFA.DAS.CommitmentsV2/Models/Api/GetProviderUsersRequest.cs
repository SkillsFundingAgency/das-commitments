namespace SFA.DAS.CommitmentsV2.Models.Api
{
    public class GetProviderUsersRequest : IGetApiRequest
    {
        public long Ukprn { get; }


        public GetProviderUsersRequest(long ukprn)
        {
            Ukprn = ukprn;
        }
        public string GetUrl => $"api/account/{Ukprn}/users";
    }
}