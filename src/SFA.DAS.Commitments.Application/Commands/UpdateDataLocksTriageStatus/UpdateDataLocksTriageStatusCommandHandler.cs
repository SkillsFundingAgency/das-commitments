using System;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus
{
    public sealed class UpdateDataLocksTriageStatusCommandHandler : AsyncRequestHandler<UpdateDataLocksTriageStatusCommand>
    {
        private readonly AbstractValidator<UpdateDataLocksTriageStatusCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public UpdateDataLocksTriageStatusCommandHandler(
            AbstractValidator<UpdateDataLocksTriageStatusCommand> validator,
            IDataLockRepository dataLockRepository)
        {
            if(validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLocksTriageStatusCommand>));
            if(dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
        }

        protected override async Task HandleCore(UpdateDataLocksTriageStatusCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var triageStatus = (TriageStatus)command.TriageStatus;

            var dataLocksToBeUpdated = (await _dataLockRepository
                .GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandled)
                .Where(DataLockExtensions.IsPriceOnly)
                .ToList();

            if (dataLocksToBeUpdated.Any(m => m.TriageStatus == triageStatus))
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with the same TriageStatus ({command.TriageStatus}) ");
            }
                
            await _dataLockRepository.UpdateDataLockTriageStatus(dataLocksToBeUpdated.Select(m => m.DataLockEventId), triageStatus);
        }
    }
}
