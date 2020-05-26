using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client.Configuration;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration
{
    public class AddEpaToApprenticeshipsConfiguration : IConfiguration // is this the base class we need/want?
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public string AssessmentOrgsApiBaseUri { get; set; }

        public PaymentsEventsApi PaymentsEventsApi { get; set; }

        public bool UsePaymentEventsDocumentRepository { get; set; }

        public bool UseAssessmentOrgsDocumentRepository { get; set; }

        public string StorageConnectionString { get; set; }
    }

    public class PaymentsEventsApi : IPaymentsEventsApiConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string IdentifierUri { get; }
        public string ClientToken { get; set; }
    }
}