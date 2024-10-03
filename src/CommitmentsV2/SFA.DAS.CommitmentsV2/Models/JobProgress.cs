namespace SFA.DAS.CommitmentsV2.Models;

public partial class JobProgress
{
    public string Lock { get; set; }
    public long? AddEpaLastSubmissionEventId { get; set; }
    public int? IntTestSchemaVersion { get; set; }
}