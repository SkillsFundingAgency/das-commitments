namespace SFA.DAS.CommitmentsV2.Models;

public class FileUploadCohortLog
{
    public long Id { get; set; } 
    public long FileUploadLogId { get; set; } 
    public long CommitmentId { get; set; } 
    public int RowCount { get; set; }
    public FileUploadLog FileUploadLog { get; set; }
}