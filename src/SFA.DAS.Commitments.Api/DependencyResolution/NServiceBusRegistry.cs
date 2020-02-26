using System;
using NServiceBus;
using SFA.DAS.Commitments.Application.Extensions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
using SFA.DAS.NServiceBus.Configuration.StructureMap;
using StructureMap;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class NServiceBusRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.Commitments";

        public NServiceBusRegistry()
        {
            For<INServiceBusConfiguration>().Use(ctx => GetConfiguration()).Singleton();
            For<IEndpointInstance>().Use(ctx => GetEndpoint(ctx)).Singleton();
        }

        private INServiceBusConfiguration GetConfiguration()
        {
            var configuration = SFA.DAS.Commitments.Infrastructure.Configuration.Configuration.Get();

            return configuration.NServiceBusConfiguration;
        }

        private IEndpointInstance GetEndpoint(IContext ctx)
        {
            var logger = ctx.GetInstance<ICommitmentsLogger>();

            try
            {
                var configuration = ctx.GetInstance<INServiceBusConfiguration>();
                var environment = ctx.GetInstance<IHostingEnvironment>();
                var container = ctx.GetInstance<IContainer>();

                logger.Info($"configuration-found?:{configuration != null} environment:{environment.EnvironmentType} nsb-transport-connection:{!string.IsNullOrWhiteSpace(configuration?.TransportConnectionString)} nsb-endpoint:{configuration.EndpointName} nsb-license:{!string.IsNullOrWhiteSpace(configuration.License)}");

                var endpointConfiguration = new EndpointConfiguration(configuration.EndpointName)
                    .UseErrorQueue()
                    .UseInstallers()
                    .UseLicense(configuration.License)
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseNLogFactory()
                    .UseStructureMapBuilder(container);

                if (environment.IsDevelopment)
                {
                    endpointConfiguration.UseLearningTransport(s => s.AddRouting());
                }
                else
                {
                    endpointConfiguration.UseAzureServiceBusTransport(configuration.TransportConnectionString, s => s.AddRouting());
                }

                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    logger.Error(ex, "Failed to get end point");
                    ex = ex.InnerException;
                }

                throw;
            }
        }
    }
}
