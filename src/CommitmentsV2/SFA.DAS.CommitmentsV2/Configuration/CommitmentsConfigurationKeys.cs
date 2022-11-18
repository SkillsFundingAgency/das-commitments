namespace SFA.DAS.CommitmentsV2.Configuration
{
    public static class CommitmentsConfigurationKeys
    {
        public const string CommitmentsV2 = "SFA.DAS.CommitmentsV2";
        public static string AccountApi => $"{CommitmentsV2}:AccountApi";
        public static string ProviderRelationshipsApi => $"{CommitmentsV2}:ProviderRelationshipsApi";
        public static string ApprenticeshipInfoService => $"{CommitmentsV2}:ApprenticeshipInfoService";
        public static string AzureActiveDirectoryApiConfiguration => $"{CommitmentsV2}:AzureADApiAuthentication";
        public static string DatabaseConnectionString => $"{CommitmentsV2}:DatabaseConnectionString";
        public static string Features => $"{CommitmentsV2}:Features";
        public static string RedisConnectionString => $"{CommitmentsV2}:RedisConnectionString";
        public static string EncodingConfiguration => "SFA.DAS.Encoding";
        public static string PasAccountApiClient => PAS.Account.Api.ClientV2.Configuration.ConfigurationKeys.PasAccountApiClient;
        public static string ApprovalsOuterApiConfiguration => $"{CommitmentsV2}:ApprovalsOuterApiConfiguration";
        public static string EmailOptionalConfiguration => $"{CommitmentsV2}:EmailOptionalConfiguration";
        public static string LevyTransferMatchingApiConfiguration => $"{CommitmentsV2}:LevyTransferMatchingInnerApiConfiguration";
        public static string ProviderAccountApiConfiguration => $"{CommitmentsV2}:ProviderAccountApiConfiguration";
        public static string CommitmentPaymentsWebJobConfiguration => $"{CommitmentsV2}:CommitmentPaymentsWebJobConfiguration";
    }
}