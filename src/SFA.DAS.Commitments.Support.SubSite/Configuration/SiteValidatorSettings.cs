using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Support.SubSite.Configuration
{
    public interface ISiteValidatorSettings
    {
        string Tenant { get; }

        string Audience { get; }

        string Scope { get; }
    }

    public class SiteValidatorSettings : ISiteValidatorSettings
    {
        public string Tenant { get; set; }

        [JsonRequired]
        public string Audience { get; set; }

        [JsonRequired]
        public string Scope { get; set; }
    }
}