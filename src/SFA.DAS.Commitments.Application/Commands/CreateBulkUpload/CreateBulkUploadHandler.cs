using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadHandler : IAsyncRequestHandler<CreateBulkUploadCommand, long>
    {
        private readonly IBulkUploadRepository _repository;

        private readonly ICommitmentsLogger _logger;

        public CreateBulkUploadHandler(
            IBulkUploadRepository repository,
            ICommitmentsLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<long> Handle(CreateBulkUploadCommand message)
        {
            _logger.Trace($"Inserting file for provider: {message.ProviderId}");

            return _repository.InsertBulkUploadFile(message.BulkUploadFile, message.FileName, message.CommitmentId);
        }
    }
}
