using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
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

        internal CohortApprovalService(IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipOverlapRules overlapRules, ICurrentDateTime currentDateTime, ICommitmentRepository commitmentRepository, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IMediator mediator)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _currentDateTime = currentDateTime;
            _commitmentRepository = commitmentRepository;
            _mediator = mediator;
            _overlappingApprenticeshipService = new OverlappingApprenticeshipService(apprenticeshipRepository, overlapRules);
            _apprenticeshipEventsService = new ApprenticeshipEventsService(apprenticeshipEventsList, apprenticeshipEventsPublisher, apprenticeshipRepository);
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

        internal async Task UpdateApprenticeships(Commitment commitment, bool isFinalApproval, AgreementStatus newAgreementStatus)
        {
            UpdateApprenticeshipStatuses(commitment, isFinalApproval, newAgreementStatus);
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
            if (isFinalApproval)
            {
                await CreatePriceHistory(commitment);
            }
        }

        internal async Task AddMessageToCommitment(Commitment commitment, string lastUpdatedByName, string messageText, CallerType createdBy)
        {
            var cohortStatusChangeService = new CohortStatusChangeService(_commitmentRepository);
            await cohortStatusChangeService.AddMessageToCommitment(commitment, lastUpdatedByName, messageText, createdBy);
        }

        internal CommitmentChangeType DetermineHistoryChangeType(bool isFinalApproval)
        {
            return isFinalApproval ? CommitmentChangeType.FinalApproval : CommitmentChangeType.SentForApproval;
        }

        internal async Task PublishApprenticeshipEvents(Commitment commitment, bool isFinalApproval)
        {
            if (!isFinalApproval)
            {
                await _apprenticeshipEventsService.PublishApprenticeshipAgreementUpdatedEvents(commitment);
            }
            else
            {
                await _apprenticeshipEventsService.PublishApprenticeshipFinalApprovalEvents(commitment);
            }
        }

        internal async Task ReorderPayments(long employerAccountId)
        {
            await _mediator.SendAsync(new SetPaymentOrderCommand { AccountId = employerAccountId });
        }

        internal Task PublishCommitmentRequiresApprovalByTransferSenderEventMessage(IMessagePublisher messagePublisher, Commitment commitment)
        {
            var senderMessage = new CommitmentRequiresApprovalByTransferSender(commitment.EmployerAccountId,
                commitment.ProviderId.Value, commitment.Id, commitment.TransferSenderId.Value);
            return messagePublisher.PublishAsync(senderMessage);
        }

        private async Task CreatePriceHistory(Commitment commitment)
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

        private void UpdateApprenticeshipStatuses(Commitment commitment, bool isFinalApproval, AgreementStatus newAgreementStatus)
        {
            var newPaymentStatus = DetermineNewPaymentStatus(isFinalApproval);
            commitment.Apprenticeships.ForEach(x =>
            {
                x.AgreementStatus = newAgreementStatus;
                x.PaymentStatus = newPaymentStatus;
                x.AgreedOn = DetermineAgreedOnDate(isFinalApproval);
            });
        }

        private DateTime? DetermineAgreedOnDate(bool isFinalApproval)
        {
            return isFinalApproval ? _currentDateTime.Now : (DateTime?)null;
        }

        private static PaymentStatus DetermineNewPaymentStatus(bool isFinalApproval)
        {
            return isFinalApproval ? PaymentStatus.Active : PaymentStatus.PendingApproval;
        }


    }
}
