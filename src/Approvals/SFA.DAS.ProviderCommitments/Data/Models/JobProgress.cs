namespace SFA.DAS.ProviderCommitments.Data.Models
{
    public partial class JobProgress
    {
        public string Lock { get; set; }
        public long? AddEpaLastSubmissionEventId { get; set; }
        public int? IntTestSchemaVersion { get; set; }
    }
}
