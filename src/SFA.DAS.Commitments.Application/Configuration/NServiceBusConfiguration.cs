using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces;

namespace SFA.DAS.Commitments.Application.Configuration
{
    public class NServiceBusConfiguration : INServiceBusConfiguration
    {
        private string _configuredEndpointName;
        private const string ServiceName = "SFA.DAS.Commitments";

        public string EndpointName
        {
            get => _configuredEndpointName ?? ServiceName;
            set => _configuredEndpointName = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public string TransportConnectionString { get; set; }
        public string License { get; set; }
    }
}
