using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.SetPaymentOrder
{
    public sealed class SetPaymentOrderCommandHandler : AsyncRequestHandler<SetPaymentOrderCommand>
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;

        public SetPaymentOrderCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (apprenticeshipEvents == null)
                throw new ArgumentNullException(nameof(apprenticeshipEvents));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
        }

        protected override async Task HandleCore(SetPaymentOrderCommand command)
        {
            _logger.Info($"Called SetPaymentOrderCommand for employer account {command.AccountId}", accountId: command.AccountId);

            var existingApprenticeships = await _commitmentRepository.GetApprenticeshipsByEmployer(command.AccountId);

            await _commitmentRepository.SetPaymentOrder(command.AccountId);

            var updatedApprenticeships = await _commitmentRepository.GetApprenticeshipsByEmployer(command.AccountId);

            await PublishEventsForApprenticeshipsWithNewPaymentOrder(command.AccountId, existingApprenticeships, updatedApprenticeships);
        }

        private async Task PublishEventsForApprenticeshipsWithNewPaymentOrder(long employerAccountId, IEnumerable<Apprenticeship> existingApprenticeships, IEnumerable<Apprenticeship> updatedApprenticeships)
        {
            var changedApprenticeships = updatedApprenticeships.Except(existingApprenticeships, new ComparerPaymentOrder()).ToList();

            _logger.Info($"Publishing {changedApprenticeships.Count} payment order events for employer account {employerAccountId}", accountId: employerAccountId);

            // TODO: Need better way to publish all these events
            await Task.WhenAll(changedApprenticeships.Select(a => _apprenticeshipEvents.PublishEvent(a, "APPRENTICESHIP-UPDATED")));
        }

        private class ComparerPaymentOrder : IEqualityComparer<Apprenticeship>
        {
            public bool Equals(Apprenticeship a, Apprenticeship b)
            {
                return a.Id == b.Id && a.PaymentOrder == b.PaymentOrder;
            }

            public int GetHashCode(Apprenticeship obj)
            {
                var hash = 19;
                hash = hash*27 + (obj.Id).GetHashCode();
                hash = hash*27 + (obj.PaymentOrder).GetHashCode();

                return hash;
            }
        }
    }
}
