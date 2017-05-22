using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadCommand : IAsyncRequest<long>
    {
        public long ProviderId { get; set; }

        public string BulkUploadFile { get; set; }
    }
}
