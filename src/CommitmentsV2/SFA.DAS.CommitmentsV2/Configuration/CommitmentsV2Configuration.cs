using System;

namespace SFA.DAS.CommitmentsV2.Configuration
{
    public class CommitmentsV2Configuration
    {
        public string DatabaseConnectionString { get; set; }

        public EventsApiClientConfiguration EventsApi { get; set; }
        public ApprenticeshipInfoServiceConfiguration ApprenticeshipInfoService { get; set; }

        public NServiceBusConfiguration NServiceBusConfiguration { get; set; }

        public AzureActiveDirectoryApiConfiguration AzureADApiAuthentication { get; set; }
        public DateTime? CurrentDateTime { get; set; }
    }
}
