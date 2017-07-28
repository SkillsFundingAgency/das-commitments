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
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage
{
    public class ApproveDataLockTriageCommandHandler : AsyncRequestHandler<ApproveDataLockTriageCommand>
    {
        private readonly AbstractValidator<ApproveDataLockTriageCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _eventsApi;
        private readonly ICurrentDateTime _currentDateTime;

        public ApproveDataLockTriageCommandHandler(AbstractValidator<ApproveDataLockTriageCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipEventsPublisher eventsApi,
            IApprenticeshipEventsList apprenticeshipEventsList,
            ICommitmentRepository commitmentRepository, ICurrentDateTime currentDateTime)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<ApproveDataLockTriageCommand>));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipRepository));
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(ICommitmentRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _eventsApi = eventsApi;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _commitmentRepository = commitmentRepository;
            _currentDateTime = currentDateTime;
        }

        protected override async Task HandleCore(ApproveDataLockTriageCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var datalocks = await _dataLockRepository.GetDataLocks(command.ApprenticeshipId);

            var dataLockPriceErrors = datalocks
                .Where(DataLockExtensions.UnHandled)
                .Where(DataLockExtensions.IsPriceOnly)
                .Where(m => m.TriageStatus == TriageStatus.Change)
                .ToList();

            var dataLockPasses = datalocks
                .Where(x => x.Status == Status.Pass
                            || (x.ErrorCode == DataLockErrorCode.Dlock07
                                && x.Status == Status.Fail
                                && x.TriageStatus == TriageStatus.Change
                                && x.IsResolved)
                );

            if (!dataLockPriceErrors.Any())
                return;

            var newPriceHistory = dataLockPriceErrors.Concat(dataLockPasses)
                .Select(m =>
                    new PriceHistory
                    {
                        ApprenticeshipId = command.ApprenticeshipId,
                        Cost = (decimal) m.IlrTotalCost,
                        FromDate = (DateTime) m.IlrEffectiveFromDate,
                        ToDate = null
                    })
                .OrderBy(x => x.FromDate)
                .ToArray();

            for (var i = 0; i < newPriceHistory.Length - 1; i++)
            {
                newPriceHistory[i].ToDate = newPriceHistory[i + 1].FromDate.AddDays(-1);
            }

            // One call to repository?
            await _apprenticeshipRepository.InsertPriceHistory(command.ApprenticeshipId, newPriceHistory);
            await _dataLockRepository.ResolveDataLock(
                dataLockPriceErrors.Select(m => m.DataLockEventId));

            await PublishEvents(command.ApprenticeshipId);
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
