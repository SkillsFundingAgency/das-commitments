namespace SFA.DAS.CommitmentsV2.Domain;

public static class Constants
{
    public static readonly DateTime TransferFeatureStartDate = new DateTime(2018, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime DasStartDate = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime MinimumDateOfBirth = new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime RecognisePriorLearningBecomesRequiredOn = new DateTime(2022, 08, 01, 0, 0, 0, DateTimeKind.Utc);

    public const int MinimumAgeAtApprenticeshipStart = 15;
    public const int MaximumAgeAtApprenticeshipStart = 115;
    public const int MaximumAgeAtApprenticeshipStartForLevel7 = 25;
    public const int MaximumApprenticeshipCost = 100000;
    public const string ServiceName = "SFA.DAS.CommitmentsV2";
    public const string IntegrationTestEnvironment = "IntegrationTest";

    /// <summary>
    ///     The maximum lengths of various fields (as defined in the database).
    /// </summary>
    public static class FieldLengths
    {
        public const int FirstName = 100;
        public const int LastName = 100;
        public const int CourseCode = 20;
        public const int ProviderReference = 50;
        public const int Uln = 50;
    }

    public static Dictionary<short, string> IlrWithdrawalReasons = new Dictionary<short, string>
    {
        { 2, "Learner has transferred to another provider" },
        { 3, "Learner injury / illness" },
        { 7, "Learner has transferred between providers due to intervention by or with the written agreement of the ESFA" },
        { 29, "Learner has been made redundant" },
        { 40, "Learner has transferred to a new learning aim with the same provider" },
        { 41, "Learner has transferred to another provider to undertake learning that meets a specific government strategy" },
        { 42, "Academic failure / left in bad standing / not permitted to progress – HE learning aims only" },
        { 43, "Financial reasons" },
        { 44, "Other personal reasons" },
        { 45, "Written off after lapse of time – HE learning aims only" },
        { 46, "Exclusion" },
        { 47, "Learner has transferred to another provider due to merger" },
        { 48, "Industry placement learner has withdrawn due to circumstances outside the providers' control" },
        { 97, "Other" },
        { 98, "Reason not known" }
    };
}