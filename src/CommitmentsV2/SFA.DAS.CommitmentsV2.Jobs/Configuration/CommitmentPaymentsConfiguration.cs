using SFA.DAS.CommitmentsV2.Domain.Interfaces;
//using SFA.DAS.Provider.Events.Api.Client.Configuration;
using System;

namespace SFA.DAS.CommitmentPaymentsV2.WebJob.Configuration
{
    public class CommitmentPaymentsConfiguration : IConfig
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        //public PaymentEventsApi PaymentEventsApi { get; set; }

        public bool UseDocumentRepository { get; set; }

        public string StorageConnectionString { get; set; }

        public bool IgnoreDataLockStatusConstraintErrors { get; set; }
    }

    //public class PaymentEventsApi : IPaymentsEventsApiConfiguration
    //{
    //    public string ApiBaseUrl { get; set; }
    //    public string Tenant { get; set; }
    //    public string ClientId { get; set; }
    //    public string ClientSecret { get; set; }
    //    public string IdentifierUri { get; set; }

    //    [Obsolete("This property is deprecated use AAD auth properties instead", true)]
    //    public string ClientToken { get; set; }
    //}
}
