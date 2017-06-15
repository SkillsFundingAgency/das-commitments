using System;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution
{
    public class UpdateDataLocksTriageResolutionHandler : AsyncRequestHandler<UpdateDataLocksTriageResolutionCommand>
    {
        private readonly AbstractValidator<UpdateDataLocksTriageResolutionCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;

        private readonly IApprenticeshipEventsPublisher _eventsApi;

        public UpdateDataLocksTriageResolutionHandler(
            AbstractValidator<UpdateDataLocksTriageResolutionCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLocksTriageResolutionCommand>));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
        }
        protected override async Task HandleCore(UpdateDataLocksTriageResolutionCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var dataLocksToBeUpdated = (await _dataLockRepository.GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandeled)
                .Where(DataLockExtensions.IsPriceOnly)
                .Where(m => m.TriageStatus == (TriageStatus)command.TriageStatus)
                .ToList();

            if (!dataLocksToBeUpdated.Any())
                return;

            if (command.DataLockUpdateType == Api.Types.DataLock.Types.DataLockUpdateType.ApproveChanges)
            {
                var newPriceHistory = dataLocksToBeUpdated
                    .Select(m => 
                        new PriceHistory
                        {
                            ApprenticeshipId = command.ApprenticeshipId,
                            Cost = (decimal)m.IlrTotalCost,
                            FromDate = (DateTime)m.IlrEffectiveFromDate,
                            ToDate = null

                        });

                
                    // One call to repository?
                    await _apprenticeshipRepository.InsertPriceHistory(command.ApprenticeshipId, newPriceHistory);
                    await _dataLockRepository.ResolveDataLock(
                        dataLocksToBeUpdated.Select(m => m.DataLockEventId));

                    // ToDo: Update events?

            }
            else if (command.DataLockUpdateType == Api.Types.DataLock.Types.DataLockUpdateType.RejectChanges)
            {
                await _dataLockRepository.UpdateDataLockTriageStatus(
                    dataLocksToBeUpdated.Select(m => m.DataLockEventId),
                    TriageStatus.Unknown);
            }
        }
    }
}