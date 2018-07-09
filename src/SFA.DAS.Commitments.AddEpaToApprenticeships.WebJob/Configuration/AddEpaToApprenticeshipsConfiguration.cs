using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.Configuration
{
    public class AddEpaToApprenticeshipsConfiguration : IConfiguration // is this the base class we need/want?
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public string AssessmentOrgsApiBaseUri { get; set; }

        public PaymentEventsApi PaymentEventsApi { get; set; }

        public bool UsePaymentEventsDocumentRepository { get; set; }

        public bool UseAssessmentOrgsDocumentRepository { get; set; }

        public string StorageConnectionString { get; set; }
    }

    public class PaymentEventsApi : IPaymentsEventsApiConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string ClientToken { get; set; }
    }
}