using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SFA.DAS.EmployerUsers.WebClientComponents;

namespace SFA.DAS.ProviderCommitments.Web.Authentication
{
    public class IdentityServerConfigurationFactory : ConfigurationFactory
    {
        private readonly ProviderCommitmentsSecurityConfiguration _configuration;

        public IdentityServerConfigurationFactory(ProviderCommitmentsSecurityConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override ConfigurationContext Get()
        {
            return new ConfigurationContext { AccountActivationUrl = _configuration.Identity.BaseAddress.Replace("/identity", "") + _configuration.Identity.AccountActivationUrl };
        }
    }
}