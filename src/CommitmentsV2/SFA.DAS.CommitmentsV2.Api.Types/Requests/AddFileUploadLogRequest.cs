namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class AddFileUploadLogRequest : SaveDataRequest
    {
        public long? ProviderId { get; set; }
        public string FileName { get; set; }
        public int? RplCount { get; set; }
        public int? RowCount { get; set; }
        public string FileContent { get; set; }
    }
}
