using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution
{
    public class UpdateDataLocksTriageResolutionHandler : AsyncRequestHandler<UpdateDataLocksTriageResolutionCommand>
    {
        private readonly AbstractValidator<UpdateDataLocksTriageResolutionCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _eventsApi;
        private readonly ICurrentDateTime _currentDateTime;

        public UpdateDataLocksTriageResolutionHandler(
            AbstractValidator<UpdateDataLocksTriageResolutionCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipEventsPublisher eventsApi,
            IApprenticeshipEventsList apprenticeshipEventsList,
            ICommitmentRepository commitmentRepository, ICurrentDateTime currentDateTime)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLocksTriageResolutionCommand>));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipRepository));
            if(commitmentRepository == null)
                throw new ArgumentNullException(nameof(ICommitmentRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _eventsApi = eventsApi;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
        }
        protected override async Task HandleCore(UpdateDataLocksTriageResolutionCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            var dataLocksToBeUpdated = (await _dataLockRepository.GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandled)
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
                        })
                        .ToArray();

                for (int i = 0; i < newPriceHistory.Length - 1; i++)
                {
                    newPriceHistory[i].ToDate = newPriceHistory[i + 1].FromDate.AddDays(-1);
                }
                
                // One call to repository?
                await _apprenticeshipRepository.InsertPriceHistory(command.ApprenticeshipId, newPriceHistory);
                await _dataLockRepository.ResolveDataLock(
                    dataLocksToBeUpdated.Select(m => m.DataLockEventId));

                await PublishEvents(command.ApprenticeshipId);


            }
            else if (command.DataLockUpdateType == Api.Types.DataLock.Types.DataLockUpdateType.RejectChanges)
            {
                await _dataLockRepository.UpdateDataLockTriageStatus(
                    dataLocksToBeUpdated.Select(m => m.DataLockEventId),
                    TriageStatus.Unknown);
            }
        }

        private async Task PublishEvents(long apprenticeshipId)
        {
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(apprenticeshipId);
            var commitment = await _commitmentRepository.GetCommitmentById(apprenticeship.CommitmentId);

            _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", _currentDateTime.Now);

            await _eventsApi.Publish(_apprenticeshipEventsList);

        } 
    }
}