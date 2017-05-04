﻿using System;
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

        public SetPaymentOrderCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, ICommitmentsLogger logger)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(apprenticeshipRepository));
            if (apprenticeshipEventsList == null)
                throw new ArgumentNullException(nameof(apprenticeshipEventsList));
            if (apprenticeshipEventsPublisher == null)
                throw new ArgumentNullException(nameof(apprenticeshipEventsPublisher));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _logger = logger;
        }

        protected override async Task HandleCore(SetPaymentOrderCommand command)
        {
            _logger.Info($"Called SetPaymentOrderCommand for employer account {command.AccountId}", accountId: command.AccountId);

            var sw = Stopwatch.StartNew();
            var existingApprenticeships = await _apprenticeshipRepository.GetApprenticeshipsByEmployer(command.AccountId);
            _logger.Trace($"Getting existing apprenticeships took {sw.ElapsedMilliseconds}");

            sw = Stopwatch.StartNew();
            await _commitmentRepository.SetPaymentOrder(command.AccountId);
            _logger.Trace($"Updating payment order took {sw.ElapsedMilliseconds}");

            sw = Stopwatch.StartNew();
            var updatedApprenticeships = await _apprenticeshipRepository.GetApprenticeshipsByEmployer(command.AccountId);
            _logger.Trace($"Getting updated apprenticeships took {sw.ElapsedMilliseconds}");

            await PublishEventsForApprenticeshipsWithNewPaymentOrder(command.AccountId, existingApprenticeships, updatedApprenticeships);
        }

        private async Task PublishEventsForApprenticeshipsWithNewPaymentOrder(long employerAccountId, IEnumerable<Apprenticeship> existingApprenticeships, IEnumerable<Apprenticeship> updatedApprenticeships)
        {
            var sw = Stopwatch.StartNew();
            var changedApprenticeships = updatedApprenticeships.Except(existingApprenticeships, new ComparerPaymentOrder()).ToList();

            _logger.Info($"Publishing {changedApprenticeships.Count} payment order events for employer account {employerAccountId}", accountId: employerAccountId);
            _logger.Trace($"Determining changed apprenticeships took {sw.ElapsedMilliseconds}");

            await PublishEventsForChangesApprenticeships(changedApprenticeships);
        }

        private async Task PublishEventsForChangesApprenticeships(List<Apprenticeship> changedApprenticeships)
        {
            var sw = Stopwatch.StartNew();
            foreach (var changedApprenticeship in changedApprenticeships)
            {
                var commitment = await _commitmentRepository.GetCommitmentById(changedApprenticeship.CommitmentId);
                _apprenticeshipEventsList.Add(commitment, changedApprenticeship, "APPRENTICESHIP-UPDATED");
            }
            _logger.Trace($"Adding events took {sw.ElapsedMilliseconds}");

            sw = Stopwatch.StartNew();
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
            _logger.Trace($"Publishing events took {sw.ElapsedMilliseconds}");
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
