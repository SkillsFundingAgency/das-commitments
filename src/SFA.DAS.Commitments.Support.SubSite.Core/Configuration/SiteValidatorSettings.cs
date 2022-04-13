using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Support.SubSite.Core.Configuration
{
    public class SiteValidatorSettings
    {
        public string Tenant { get; set; }

        [JsonRequired]
        public string Audience { get; set; }

        [JsonRequired]
        public string Scope { get; set; }
    }
}