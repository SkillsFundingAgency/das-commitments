namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class BulkUpload
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string FileName { get; set; }
        public string FileContent { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
