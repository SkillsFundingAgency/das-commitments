using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.CommitmentPayments.WebJob.Configuration
{
    public class CommitmentPaymentsConfiguration : IConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
    }
}
