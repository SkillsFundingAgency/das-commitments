using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetBulkUploadFile
{
    public class GetBulkUploadFileQuery : IAsyncRequest<GetBulkUploadFileResponse>
    {
        public Caller Caller { get; set; }

        public long ProviderId { get; set; }

        public long BulkUploadFileId { get; set; }

    }
}