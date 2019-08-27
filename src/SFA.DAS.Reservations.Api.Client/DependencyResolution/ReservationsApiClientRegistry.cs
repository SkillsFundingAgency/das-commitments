using System;
using System.Net.Http;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        public ReservationsApiClientRegistry()
        {
            For<ReservationsClientApiConfiguration>().Use(ctx => GetConfig(ctx)).Singleton();
            For<IReservationsApiClient>().Use(ctx => CreateClient(ctx)).Singleton();
            For<IReservationHelper>().Use<ReservationHelper>().Singleton();
        }

        public ReservationsClientApiConfiguration GetConfig(IContext ctx)
        {
            var configurationRepository = ctx.GetInstance<IConfigurationRepository>();
            var environmentName = GetEnvironmentName();
            var configurationOptions =
                new ConfigurationOptions(ConfigurationKeys.ReservationsClientApiConfiguration, environmentName, "1.0");

            var svc = new ConfigurationService(configurationRepository, configurationOptions);

            return svc.Get<ReservationsClientApiConfiguration>();
        }

        private string GetEnvironmentName()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            return environment;
        }

        private IReservationsApiClient CreateClient(IContext ctx)
        {
            var config = ctx.GetInstance<ReservationsClientApiConfiguration>();

            HttpClient httpClient;

            if (config.UseStub)
            {
                httpClient = new HttpClient();
            }
            else
            {
                var adConfig = new ReservationsClientApiConfigurationADAdapter(config);
                var bearerToken = new AzureADBearerTokenGenerator(adConfig);

                httpClient = new HttpClientBuilder()
                    .WithBearerAuthorisationHeader(bearerToken)
                    .WithHandler(new NLog.Logger.Web.MessageHandlers.RequestIdMessageRequestHandler())
                    .WithHandler(new NLog.Logger.Web.MessageHandlers.SessionIdMessageRequestHandler())
                    .WithDefaultHeaders()
                    .Build();
            }

            For<IReservationsApiClient>().Use<ReservationsApiClient>().Ctor<HttpClient>().Is(httpClient).Singleton();
            var helper = ctx.GetInstance<IReservationHelper>();
            var log = ctx.GetInstance<ILog>();
            return new ReservationsApiClient(httpClient, helper, log);
        }
    }
}
