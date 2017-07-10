using System.Threading.Tasks;

using FluentValidation;
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
            var result = await _repository.GetBulkUploadFile(message.BulkUploadFileId);

            if (result.ProviderId != message.ProviderId)
                throw new ValidationException($"Provider {message.ProviderId} cannot access bulk upload {message.BulkUploadFileId}");
            
            return new GetBulkUploadFileResponse { Data = result.FileContent };
        }
    }
}