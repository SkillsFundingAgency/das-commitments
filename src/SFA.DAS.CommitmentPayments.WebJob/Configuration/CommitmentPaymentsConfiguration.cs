﻿using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.CommitmentPayments.WebJob.Configuration
{
    public class CommitmentPaymentsConfiguration : IConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public PaymentEventsApi PaymentEventsApi { get; set; }

        public bool UseDocumentRepository { get; set; }

        public string StorageConnectionString { get; set; }
    }

    public class PaymentEventsApi : IPaymentsEventsApiConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string ClientToken { get; set; }
    }
}
