using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetBulkUploadFile
{
    public class GetBulkUploadFileQuery : IAsyncRequest<GetBulkUploadFileResponse>
    {
        public long ProviderId { get; set; }

        public long BulkUploadFileId { get; set; }
    }
}