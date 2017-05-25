using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadCommand : IAsyncRequest<long>
    {
        public long ProviderId { get; set; }

        public long CommitmentId { get; set; }

        public string FileName { get; set; }

        public string BulkUploadFile { get; set; }
    }
}
