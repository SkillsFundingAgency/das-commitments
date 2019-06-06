namespace SFA.DAS.CommitmentsV2.Configuration
{
    public static class CommitmentsConfigurationKeys
    {
        public const string CommitmentsV2 = "SFA.DAS.CommitmentsV2";
        public static string ApprenticeshipInfoService => $"{CommitmentsV2}:ApprenticeshipInfoService";
        public static string AzureActiveDirectoryApiConfiguration => $"{CommitmentsV2}:AzureADApiAuthentication";
        public static string CommitmentIdHashingConfiguration => $"{CommitmentsV2}:CommitmentIdHashing";
        public static string Features => $"{CommitmentsV2}:Features";
        public static string ReservationsClientApiConfiguration => $"{CommitmentsV2}:ReservationsClientApi";
    }
}
