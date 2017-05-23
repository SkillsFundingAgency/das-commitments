using System;

namespace SFA.DAS.Commitments.Api.Models
{
    public static class SystemDetails
    {
        public static string VersionNumber { get; set; }
        public static string EnvironmentName { get; set; }
    }

    public class BulkUploadFile
    {
        public long CommitmentId { get; set; }

        public string FileName { get; set; }

        public string Data { get; set; }
    }
}
