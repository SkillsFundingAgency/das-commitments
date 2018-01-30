using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands.TriageDataLocks
{
    public sealed class TriageDataLocksCommandHandler : AsyncRequestHandler<TriageDataLocksCommand>
    {
        private readonly AbstractValidator<TriageDataLocksCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public TriageDataLocksCommandHandler(
            AbstractValidator<TriageDataLocksCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        protected override async Task HandleCore(TriageDataLocksCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var dataLocksToBeUpdated = (await _dataLockRepository
                .GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandled)
                .ToList();

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            Validate(command, dataLocksToBeUpdated, apprenticeship);

            if (apprenticeship.HasHadDataLockSuccess && command.TriageStatus == TriageStatus.Change)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly).ToList();
            }

            if (dataLocksToBeUpdated.Any(m => m.TriageStatus == command.TriageStatus))
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with the same TriageStatus ({command.TriageStatus}) ");
            }

            await _dataLockRepository.UpdateDataLockTriageStatus(dataLocksToBeUpdated.Select(m => m.DataLockEventId), command.TriageStatus);
        }

        private static void Validate(TriageDataLocksCommand command, List<DataLockStatus> dataLocksToBeUpdated, Apprenticeship apprenticeship)
        {
            var courseAndPriceOrOnlyCourse = dataLocksToBeUpdated.All(DataLockExtensions.WithCourseError)
                                          || dataLocksToBeUpdated.Any(DataLockExtensions.WithCourseAndPriceError);
            if (   courseAndPriceOrOnlyCourse
                && apprenticeship.HasHadDataLockSuccess
                && command.TriageStatus == TriageStatus.Change)
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with triage status ({command.TriageStatus}) and datalock with course and price when Successful DataLock already received");
            }
        }
    }
}
