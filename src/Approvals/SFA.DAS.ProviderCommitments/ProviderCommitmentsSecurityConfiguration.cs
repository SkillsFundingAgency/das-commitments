using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.ProviderCommitments.Configuration;

namespace SFA.DAS.ProviderCommitments
{
    public class ProviderCommitmentsSecurityConfiguration
    {
        public IdentityServerConfiguration Identity { get; set; }
        public string DashboardUrl { get; set; }
    }
}
