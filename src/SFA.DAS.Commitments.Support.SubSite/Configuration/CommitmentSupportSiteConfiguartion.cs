namespace SFA.DAS.Commitments.Support.SubSite.Configuration
{
    public class CommitmentSupportSiteConfiguartion
    {
        public string DatabaseConnectionString { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public string Hashstring { get; set; }

        public SiteValidatorSettings SiteValidator { get; set; } = new SiteValidatorSettings();
    }
}