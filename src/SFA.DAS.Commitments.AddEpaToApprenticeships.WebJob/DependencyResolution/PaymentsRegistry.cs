using System.Net.Http;
using SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NLog.Logger.Web.MessageHandlers;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Client.Configuration;
using StructureMap;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
{
    internal class PaymentsRegistry : Registry
    {
        public PaymentsRegistry()
        {
            For<PaymentEventsApi>().Use(c => c.GetInstance<AddEpaToApprenticeshipsConfiguration>().PaymentEventsApi);
            For<IPaymentsEventsApiConfiguration>().Use(c => c.GetInstance<PaymentEventsApi>());
            For<IPaymentsEventsApiClient>().Use<PaymentsEventsApiClient>().Ctor<HttpClient>().Is(c => CreateClient(c));
        }

        private HttpClient CreateClient(IContext context)
        {
            var config = context.GetInstance<AddEpaToApprenticeshipsConfiguration>().PaymentEventsApi;

            HttpClient httpClient = new HttpClientBuilder()
                    .WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(config))
                    .WithHandler(new RequestIdMessageRequestHandler())
                    .WithHandler(new SessionIdMessageRequestHandler())
                    .WithDefaultHeaders()
                    .Build();


            return httpClient;
        }
    }
}
