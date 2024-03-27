using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders
{
    public class GetAllProvidersQueryResult
    {
        public List<Provider> Providers { get; set; }

        public GetAllProvidersQueryResult(List<Provider> providers)
        {
            Providers = providers;
        }
    }
}