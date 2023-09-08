using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class FileUploadLog
    {
        public FileUploadLog()
        {
            CohortLogs = new List<FileUploadCohortLog>();
        }
        public long Id { get; set; } 
        public long? ProviderId { get; set; } 
        public string FileName { get; set; } 
        public int? RplCount { get; set; } 
        public int? RowCount { get; set; } 
        public string ProviderAction { get; set; } 
        public string FileContent { get; set; } 
        public DateTime CreatedOn { get; set; } 
        public DateTime? CompletedOn { get; set; } 
        public string Error { get; set; }
        public virtual ICollection<FileUploadCohortLog> CohortLogs { get; set; }
    }
}