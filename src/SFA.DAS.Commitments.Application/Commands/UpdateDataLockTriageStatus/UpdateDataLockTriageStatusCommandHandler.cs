using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommandHandler : AsyncRequestHandler<UpdateDataLockTriageStatusCommand>
    {
        private readonly AbstractValidator<UpdateDataLockTriageStatusCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public UpdateDataLockTriageStatusCommandHandler(AbstractValidator<UpdateDataLockTriageStatusCommand> validator,
            IDataLockRepository dataLockRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLockTriageStatusCommand>));
            if(dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
        }

        protected override async Task HandleCore(UpdateDataLockTriageStatusCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var triageStatus = (TriageStatus) message.TriageStatus;

            await _dataLockRepository.UpdateDataLockTriageStatus(message.DataLockEventId, triageStatus);
        }
    }
}
