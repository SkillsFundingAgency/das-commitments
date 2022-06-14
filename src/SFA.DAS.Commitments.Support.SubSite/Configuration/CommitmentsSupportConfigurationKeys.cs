namespace SFA.DAS.Commitments.Support.SubSite.Configuration
{
    public static class CommitmentsSupportConfigurationKeys
    {
        public const string CommitmentsSupportSubSite = "SFA.DAS.Support.Commitments";
        public static string RedisConnectionString => $"{CommitmentsSupportSubSite}:RedisConnectionString";
    }
}