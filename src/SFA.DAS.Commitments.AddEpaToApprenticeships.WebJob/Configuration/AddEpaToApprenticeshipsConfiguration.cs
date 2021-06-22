using SFA.DAS.Commitments.Domain.Configuration;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client.Configuration;
using System;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration
{
    public class AddEpaToApprenticeshipsConfiguration : IConfiguration // is this the base class we need/want?
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public ApprovalsOuterApiConfiguration ApprovalsOuterApiConfiguration { get; set; }

        public PaymentEventsApi PaymentEventsApi { get; set; }

        public bool UsePaymentEventsDocumentRepository { get; set; }

        public bool UseAssessmentOrgsDocumentRepository { get; set; }

        public string StorageConnectionString { get; set; }
    }

    public class PaymentEventsApi : IPaymentsEventsApiConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }

        [Obsolete("This property is deprecated use AAD auth properties instead", true)]
        public string ClientToken { get; set; }
    }
}