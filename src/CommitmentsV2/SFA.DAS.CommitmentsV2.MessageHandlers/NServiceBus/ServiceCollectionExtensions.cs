using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.CommitmentsV2.MessageHandlers.Configuration;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.NServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton<IEndpointInstance>(s =>
                {
                    var container = s.GetService<IContainer>();
                    var hostingEnvironment = s.GetService<IHostingEnvironment>();

                    // This will need to be resolved or injected
                    var configurationSection = new CommitmentsV2Configuration();
                    configurationSection.ServiceBusConnectionString =
                        "Endpoint=sb://das-at-shared-ns.servicebus.windows.net/;SharedAccessKeyName=ReadWrite;SharedAccessKey=XXXXXXXXXXXXX=";
                    configurationSection.NServiceBusLicense =
                        "&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&lt;license type=&quot;Commercial&quot; DeploymentType=&quot;Elastic Cloud&quot; Quantity=&quot;15&quot; Edition=&quot;Advanced&quot; Applications=&quot;NServiceBus;ServiceControl;ServicePulse&quot; RenewalType=&quot;Subscription&quot; expiration=&quot;2019-08-08&quot; id=&quot;220a2e42-6ca3-4e3f-ada3-bd370e317055&quot;&gt;  &lt;name&gt;Education &amp;amp; Skills Funding Agency&lt;/name&gt;  &lt;Signature xmlns=&quot;http://www.w3.org/2000/09/xmldsig#&quot;&gt;    &lt;SignedInfo&gt;      &lt;CanonicalizationMethod Algorithm=&quot;http://www.w3.org/TR/2001/REC-xml-c14n-20010315&quot; /&gt;      &lt;SignatureMethod Algorithm=&quot;http://www.w3.org/2000/09/xmldsig#rsa-sha1&quot; /&gt;      &lt;Reference URI=&quot;&quot;&gt;        &lt;Transforms&gt;          &lt;Transform Algorithm=&quot;http://www.w3.org/2000/09/xmldsig#enveloped-signature&quot; /&gt;        &lt;/Transforms&gt;        &lt;DigestMethod Algorithm=&quot;http://www.w3.org/2000/09/xmldsig#sha1&quot; /&gt;        &lt;DigestValue&gt;XHah5SBKh2JMIHm4koeiv07aRQk=&lt;/DigestValue&gt;      &lt;/Reference&gt;    &lt;/SignedInfo&gt;    &lt;SignatureValue&gt;o7OjJ87u5SrcXUcR1xaPV077TfYaodfqso7216AoVP8Nkhs/oMTfdnnZxROe/J7tqUpbIbDNsXD87jUErRPJchLGlsNWDOzyg7ygWhsTMOhZSV9rxa3q8BC3CiyFF2eMDO2CLJtBurf+58qIGqW+dWfz8qtDI3+gdQk3P8NPdhI=&lt;/SignatureValue&gt;  &lt;/Signature&gt;&lt;/license&gt;";

                    var isDevelopment = hostingEnvironment.IsDevelopment();

                    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.MessageHandlers")
                        .UseAzureServiceBusTransport(() => configurationSection.ServiceBusConnectionString, isDevelopment)
                        .UseInstallers()
                        .UseLicense(configurationSection.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseInstallers()
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();

                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    return endpoint;
                });
        }
    }
}