using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class FileUploadCohortLog
    {
		public long Id { get; set; } 
		public long? LogId { get; set; } 
		public long? CommitmentId { get; set; } 
		public long? RowCount { get; set; } 
    }
}