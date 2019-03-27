using System.Data.Common;
using NServiceBus;
using SFA.DAS.Commitments.Application.Configuration;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Configuration;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.StructureMap;
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
                var configuration = ctx.GetInstance<INServiceBusConfiguration>();
                var environment = ctx.GetInstance<IHostingEnvironment>();
                var container = ctx.GetInstance<IContainer>();

                var endpointConfiguration = new EndpointConfiguration(configuration.EndpointName)
                    .UseAzureServiceBusTransport(() => configuration.TransportConnectionString, environment.IsDevelopment)
                    .UseErrorQueue()
                    .UseInstallers()
                    .UseLicense(configuration.License)
                    .UseDasMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseNLogFactory()
                    .UseStructureMapBuilder(container);

                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
        }
    }
}
