namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class AddFileUploadLogRequest : SaveDataRequest
    {
        public long? ProviderId { get; }
        public string FileName { get; }
        public int? RplCount { get; }
        public int? RowCount { get; }
        public string FileContent { get; }
    }
}
