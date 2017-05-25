using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetBulkUploadFile
{
    public sealed class GetBulkUploadFileHandler : IAsyncRequestHandler<GetBulkUploadFileQuery, GetBulkUploadFileResponse>
    {
        private readonly IBulkUploadRepository _repository;

        public GetBulkUploadFileHandler(
            IBulkUploadRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetBulkUploadFileResponse> Handle(GetBulkUploadFileQuery message)
        {
            var file = await _repository.GetBulkUploadFile(message.BulkUploadFileId);

            return new GetBulkUploadFileResponse { Data = file };
        }
    }
}