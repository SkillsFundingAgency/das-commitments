using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands.TriageDataLocks
{
    public sealed class TriageDataLockCommandHandler : AsyncRequestHandler<TriageDataLockCommand>
    {
        private readonly AbstractValidator<TriageDataLockCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public TriageDataLockCommandHandler(
            AbstractValidator<TriageDataLockCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        protected override async Task HandleCore(TriageDataLockCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var dataLocksToBeUpdated = (await _dataLockRepository
                .GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandled);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            if (apprenticeship.HasHadDataLockSuccess)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly);
            }

            if (dataLocksToBeUpdated.Any(m => m.TriageStatus == command.TriageStatus))
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with the same TriageStatus ({command.TriageStatus}) ");
            }
                
            await _dataLockRepository.UpdateDataLockTriageStatus(dataLocksToBeUpdated.Select(m => m.DataLockEventId), command.TriageStatus);
        }
    }
}
