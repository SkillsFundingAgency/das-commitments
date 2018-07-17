using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class CohortApprovalService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMediator _mediator;
        private readonly OverlappingApprenticeshipService _overlappingApprenticeshipService;
        private readonly ApprenticeshipEventsService _apprenticeshipEventsService;
        private readonly ICommitmentsLogger _logger;

        internal CohortApprovalService(IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules,
            ICurrentDateTime currentDateTime,
            ICommitmentRepository commitmentRepository,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher,
            IMediator mediator,
            ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _currentDateTime = currentDateTime;
            _commitmentRepository = commitmentRepository;
            _mediator = mediator;
            _logger = logger;
            _overlappingApprenticeshipService = new OverlappingApprenticeshipService(apprenticeshipRepository, overlapRules);
            _apprenticeshipEventsService = new ApprenticeshipEventsService(apprenticeshipEventsList,
                apprenticeshipEventsPublisher,
                apprenticeshipRepository,
                logger);
        }

        internal void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus == CommitmentStatus.Deleted)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
            }
        }

        internal async Task CheckOverlaps(Commitment commitment)
        {
            if (await _overlappingApprenticeshipService.CommitmentHasOverlappingApprenticeships(commitment))
            {
                throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
            }
        }

        internal AgreementStatus GetCurrentAgreementStatus(Commitment commitment)
        {
            // Comment 1: This assumes, correctly, that during approval all apprenticeships have the same status.
            // Comment 2: I hate comments.
            return commitment.Apprenticeships.First().AgreementStatus;
        }

        internal async Task UpdateApprenticeships(Commitment commitment, bool haveBothPartiesApproved, AgreementStatus newAgreementStatus)
        {
            SetApprenticeshipsPaymentAndAgreementStatuses(commitment, haveBothPartiesApproved, newAgreementStatus);
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
            if (haveBothPartiesApproved)
            {
                if (!commitment.HasTransferSenderAssigned)
                {
                    await CreatePriceHistory(commitment);
                }
            }
        }

        internal async Task UpdateApprenticeshipsPaymentStatusToPaid(Commitment commitment)
        {
            commitment.Apprenticeships.ForEach(x =>
            {
                x.PaymentStatus = PaymentStatus.Active;
            });
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
        }

        internal async Task ResetApprenticeshipsAgreementStatuses(Commitment commitment)
        {
            commitment.Apprenticeships.ForEach(x =>
            {
                x.AgreementStatus = AgreementStatus.NotAgreed;
            });
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
        }

        internal async Task AddMessageToCommitment(Commitment commitment, string lastUpdatedByName, string messageText, CallerType createdBy)
        {
            var cohortStatusChangeService = new CohortStatusChangeService(_commitmentRepository);
            await cohortStatusChangeService.AddMessageToCommitment(commitment, lastUpdatedByName, messageText, createdBy);
        }

        internal CommitmentChangeType DetermineHistoryChangeType(bool haveBothPartiesApproved)
        {
            return haveBothPartiesApproved ? CommitmentChangeType.FinalApproval : CommitmentChangeType.SentForApproval;
        }

        internal async Task PublishApprenticeshipEvents(Commitment commitment, bool haveBothPartiesApproved)
        {
            if (!haveBothPartiesApproved || commitment.HasTransferSenderAssigned)
            {
                await _apprenticeshipEventsService.PublishApprenticeshipAgreementUpdatedEvents(commitment);
            }
            else
            {
                await _apprenticeshipEventsService.PublishApprenticeshipFinalApprovalEvents(commitment);
            }
        }

        internal async Task PublishApprenticeshipEventsWhenTransferSenderHasApproved(Commitment commitment)
        {
            await _apprenticeshipEventsService.PublishApprenticeshipFinalApprovalEvents(commitment);
        }

        internal async Task ReorderPayments(long employerAccountId)
        {
            await _mediator.SendAsync(new SetPaymentOrderCommand { AccountId = employerAccountId });
        }

        internal decimal CurrentCostOfCohort(Commitment commitment)
        {
            return commitment.Apprenticeships.Sum(x => x.Cost ?? 0);
        }

        internal List<TrainingCourseSummary> TrainingCourseSummaries(Commitment commitment)
        {
            var apprenticeships = commitment.Apprenticeships ?? new List<Apprenticeship>();

            var grouped = apprenticeships.GroupBy(l => l.TrainingCode).Select(cl =>
                new TrainingCourseSummary
                {
                    CourseTitle = cl.First().TrainingName,
                    ApprenticeshipCount = cl.Count()
                });

            return grouped.ToList();
        }

        internal Task PublishCommitmentRequiresApprovalByTransferSenderEventMessage(IMessagePublisher messagePublisher, Commitment commitment, long transferRequestId)
        {
            decimal totalCost = CurrentCostOfCohort(commitment);

            var senderMessage = new CohortApprovalByTransferSenderRequested(transferRequestId, commitment.EmployerAccountId,
                commitment.Id, commitment.TransferSenderId.Value, totalCost);
            return messagePublisher.PublishAsync(senderMessage);
        }

        internal async Task CreatePriceHistory(Commitment commitment)
        {
            await _apprenticeshipRepository.CreatePriceHistoryForApprenticeshipsInCommitment(commitment.Id);

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                apprenticeship.PriceHistory = new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = apprenticeship.Id, Cost = apprenticeship.Cost.Value, FromDate = apprenticeship.StartDate.Value }
                };
            }
        }

        private void SetApprenticeshipsPaymentAndAgreementStatuses(Commitment commitment, bool haveBothPartiesApproved, AgreementStatus newAgreementStatus)
        {
            var newPaymentStatus = DetermineNewPaymentStatus(commitment, haveBothPartiesApproved);
            commitment.Apprenticeships.ForEach(x =>
            {
                x.AgreementStatus = newAgreementStatus;
                x.PaymentStatus = newPaymentStatus;
                x.AgreedOn = DetermineAgreedOnDate(haveBothPartiesApproved);
            });
        }

        private DateTime? DetermineAgreedOnDate(bool haveBothPartiesApproved)
        {
            return haveBothPartiesApproved ? _currentDateTime.Now : (DateTime?)null;
        }

        private static PaymentStatus DetermineNewPaymentStatus(Commitment commitment, bool haveBothPartiesApproved)
        {
            return haveBothPartiesApproved && !commitment.HasTransferSenderAssigned ? PaymentStatus.Active : PaymentStatus.PendingApproval;
        }


    }
}
