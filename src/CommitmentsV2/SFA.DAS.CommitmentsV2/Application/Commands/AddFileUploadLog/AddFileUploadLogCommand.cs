using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog
{
    public class AddFileUploadLogCommand : IRequest<AddFileUploadLogResult>
    {
        public long? ProviderId { get; set; }
        public string FileName { get; set; }
        public int? RplCount { get; set; }
        public int? RowCount { get; set; }
        public string FileContent { get; set; }

        public AddFileUploadLogCommand(long? providerId, string fileName, int? rplCount, int? rowCount, string fileContent)
        {
            ProviderId = providerId;
            FileName = fileName;
            RplCount = rplCount;
            RowCount = rowCount;
            FileContent = fileContent;
        }
    }
}