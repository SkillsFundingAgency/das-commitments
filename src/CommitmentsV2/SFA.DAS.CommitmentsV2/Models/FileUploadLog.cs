using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class FileUploadLog
    {
		public long Id { get; set; } 
		public long? ProviderId { get; set; } 
		public string FileName { get; set; } 
		public long? RplCount { get; set; } 
		public long? RowCount { get; set; } 
		public string ProviderAction { get; set; } 
		public string FileContent { get; set; } 
		public DateTime CreatedOn { get; set; } 
		public DateTime? CompletedOn { get; set; } 
		public string Error { get; set; } 
    }
}