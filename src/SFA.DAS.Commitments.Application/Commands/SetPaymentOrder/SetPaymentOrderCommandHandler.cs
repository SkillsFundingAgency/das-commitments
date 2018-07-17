using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.SetPaymentOrder
{
    public sealed class SetPaymentOrderCommandHandler : AsyncRequestHandler<SetPaymentOrderCommand>
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly ICurrentDateTime _currentDateTime;

        public SetPaymentOrderCommandHandler(
            ICommitmentRepository commitmentRepository, 
            IApprenticeshipRepository apprenticeshipRepository, 
            IApprenticeshipEventsList apprenticeshipEventsList, 
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, 
            ICommitmentsLogger logger,
            ICurrentDateTime currentDateTime)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        protected override async Task HandleCore(SetPaymentOrderCommand command)
        {
            _logger.Info($"Setting PaymentOrder for employer account {command.AccountId}", accountId: command.AccountId);

            var existingApprenticeships = await _apprenticeshipRepository.GetApprenticeshipsByEmployer(command.AccountId);
            
            await _commitmentRepository.SetPaymentOrder(command.AccountId);
            
            var updatedApprenticeships = await _apprenticeshipRepository.GetApprenticeshipsByEmployer(command.AccountId);
            
            await PublishEventsForApprenticeshipsWithNewPaymentOrder(command.AccountId, existingApprenticeships.Apprenticeships, updatedApprenticeships.Apprenticeships);
        }

        private async Task PublishEventsForApprenticeshipsWithNewPaymentOrder(long employerAccountId, IEnumerable<Apprenticeship> existingApprenticeships, IEnumerable<Apprenticeship> updatedApprenticeships)
        {
            var sw = Stopwatch.StartNew();
            var changedApprenticeships = updatedApprenticeships.Except(existingApprenticeships, new ComparerPaymentOrder()).ToList();

            if (!changedApprenticeships.Any())
            {
                _logger.Info("No changed apprenticeships; no events to publish");
                return;
            }

            _logger.Info($"Publishing {changedApprenticeships.Count} payment order events for employer account {employerAccountId}", accountId: employerAccountId);
            await PublishEventsForChangesApprenticeships(changedApprenticeships);
        }

        private async Task PublishEventsForChangesApprenticeships(List<Apprenticeship> changedApprenticeships)
        {
            var commitments = await GetCommitmentsForApprenticeships(changedApprenticeships);

            var sw = Stopwatch.StartNew();
            foreach (var changedApprenticeship in changedApprenticeships)
            {
                var commitment = commitments[changedApprenticeship.CommitmentId];
                _apprenticeshipEventsList.Add(commitment, changedApprenticeship, "APPRENTICESHIP-UPDATED", _currentDateTime.Now.Date, null);
            }
            
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private async Task<Dictionary<long, Commitment>> GetCommitmentsForApprenticeships(List<Apprenticeship> changedApprenticeships)
        {
            var commitments = new Dictionary<long, Commitment>();
            var commitmentIds = changedApprenticeships.Select(x => x.CommitmentId).Distinct();
            var tasks = commitmentIds.Select(x => _commitmentRepository.GetCommitmentById(x)).ToList();
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                var commitment = task.Result;
                commitments.Add(commitment.Id, commitment);
            }
            return commitments;
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
                hash = hash * 27 + (obj.Id).GetHashCode();
                hash = hash * 27 + (obj.PaymentOrder).GetHashCode();

                return hash;
            }
        }
    }
}
