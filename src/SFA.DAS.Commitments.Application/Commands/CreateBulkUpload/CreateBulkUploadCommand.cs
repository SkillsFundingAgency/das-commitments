using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadCommand : IAsyncRequest<long>
    {
        public Caller Caller { get; set; }

        public long ProviderId { get; set; }

        public long CommitmentId { get; set; }

        public string FileName { get; set; }

        public string BulkUploadFile { get; set; }

    }
}
