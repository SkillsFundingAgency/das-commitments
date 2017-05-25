using System.Threading.Tasks;

using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadHandler : IAsyncRequestHandler<CreateBulkUploadCommand, long>
    {
        private readonly AbstractValidator<CreateBulkUploadCommand> _validator;

        private readonly IBulkUploadRepository _repository;

        private readonly ICommitmentsLogger _logger;

        public CreateBulkUploadHandler(
            AbstractValidator<CreateBulkUploadCommand> validator,
            IBulkUploadRepository repository,
            ICommitmentsLogger logger)
        {
            _validator = validator;
            _repository = repository;
            _logger = logger;
        }

        public Task<long> Handle(CreateBulkUploadCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            _logger.Trace($"Inserting file for provider: {command.ProviderId}");

            return _repository.InsertBulkUploadFile(command.BulkUploadFile, command.FileName, command.CommitmentId);
        }
    }
}