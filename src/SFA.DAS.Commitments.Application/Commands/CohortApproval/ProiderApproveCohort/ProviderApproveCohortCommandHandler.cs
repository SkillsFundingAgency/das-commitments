using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort
{
    public sealed class ProviderApproveCohortCommandHandler : AsyncRequestHandler<ProviderApproveCohortCommand>
    {
        private readonly AbstractValidator<ProviderApproveCohortCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ICommitmentsLogger _logger;
        private readonly CohortApprovalService _cohortApprovalService;
        private readonly HistoryService _historyService;

        public ProviderApproveCohortCommandHandler(AbstractValidator<ProviderApproveCohortCommand> validator, ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipOverlapRules overlapRules, ICurrentDateTime currentDateTime, IHistoryRepository historyRepository, IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IMediator mediator, IMessagePublisher messagePublisher, ICommitmentsLogger logger)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _historyService = new HistoryService(historyRepository);
            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository, overlapRules, currentDateTime, commitmentRepository, apprenticeshipEventsList, apprenticeshipEventsPublisher, mediator, _logger);
        }

        protected override async Task HandleCore(ProviderApproveCohortCommand message)
        {
            _validator.ValidateAndThrow(message);

            var commitment = await GetCommitment(message.CommitmentId);
            await CheckCommitmentCanBeApproved(commitment, message.Caller.Id);

            var haveBothPartiesApproved = HaveBothPartiesApproved(commitment);
            var newAgreementStatus = DetermineNewAgreementStatus(haveBothPartiesApproved);

            await UpdateCommitment(commitment, haveBothPartiesApproved, message.UserId, message.LastUpdatedByName, message.LastUpdatedByEmail, message.Message);

            await _cohortApprovalService.UpdateApprenticeships(commitment, haveBothPartiesApproved, newAgreementStatus);            

            if (haveBothPartiesApproved)
            {
                if (commitment.HasTransferSenderAssigned)
                {
                    var transferRequestId = await _commitmentRepository.StartTransferRequestApproval(commitment.Id,
                        _cohortApprovalService.CurrentCostOfCohort(commitment),
                        _cohortApprovalService.TrainingCourseSummaries(commitment));

                    await _cohortApprovalService.PublishCommitmentRequiresApprovalByTransferSenderEventMessage(_messagePublisher, commitment, transferRequestId);

                    commitment.TransferApprovalStatus = TransferApprovalStatus.Pending;
                }
            }
            else
            {
                await PublishApprovalRequestedMessage(commitment);
            }

            await _cohortApprovalService.PublishApprenticeshipEvents(commitment, haveBothPartiesApproved);

            if (haveBothPartiesApproved && !commitment.HasTransferSenderAssigned)
            {
                await _cohortApprovalService.ReorderPayments(commitment.EmployerAccountId);
            }
        }

        private async Task PublishApprovalRequestedMessage(Commitment commitment)
        {
            await _messagePublisher.PublishAsync(new CohortApprovalRequestedByProvider(commitment.EmployerAccountId, commitment.ProviderId.Value, commitment.Id));
        }

        private async Task UpdateCommitment(Commitment commitment, bool haveBothPartiesApproved, string userId, string lastUpdatedByName, string lastUpdatedByEmail, string message)
        {
            var updatedEditStatus = DetermineNewEditStatus(haveBothPartiesApproved);
            var changeType = _cohortApprovalService.DetermineHistoryChangeType(haveBothPartiesApproved);
            _historyService.TrackUpdate(commitment, changeType.ToString(), commitment.Id, null, CallerType.Provider, userId, commitment.ProviderId, commitment.EmployerAccountId, lastUpdatedByName);

            commitment.EditStatus = updatedEditStatus;
            commitment.LastAction = LastAction.Approve;
            commitment.CommitmentStatus = CommitmentStatus.Active;
            commitment.LastUpdatedByProviderEmail = lastUpdatedByEmail;
            commitment.LastUpdatedByProviderName = lastUpdatedByName;

            await Task.WhenAll(
                _cohortApprovalService.AddMessageToCommitment(commitment, lastUpdatedByName, message, CallerType.Provider),
                _commitmentRepository.UpdateCommitment(commitment),
                _historyService.Save()
            );
        }

        private EditStatus DetermineNewEditStatus(bool haveBothPartiesApproved)
        {
            return haveBothPartiesApproved ? EditStatus.Both : EditStatus.EmployerOnly;
        }

        private static AgreementStatus DetermineNewAgreementStatus(bool haveBothPartiesApproved)
        {
            var newAgreementStatus = haveBothPartiesApproved ? AgreementStatus.BothAgreed : AgreementStatus.ProviderAgreed;
            return newAgreementStatus;
        }

        private bool HaveBothPartiesApproved(Commitment commitment)
        {
            var currentAgreementStatus = _cohortApprovalService.GetCurrentAgreementStatus(commitment);
            return currentAgreementStatus == AgreementStatus.EmployerAgreed;
        }

        private async Task<Commitment> GetCommitment(long commitmentId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(commitmentId);
            return commitment;
        }

        private async Task CheckCommitmentCanBeApproved(Commitment commitment, long callerEmployerAccountId)
        {
            _cohortApprovalService.CheckCommitmentStatus(commitment);
            CheckEditStatus(commitment);
            CheckAuthorization(callerEmployerAccountId, commitment);
            CheckStateForApproval(commitment);
            await _cohortApprovalService.CheckOverlaps(commitment);
        }

        private static void CheckEditStatus(Commitment commitment)
        {
            if (commitment.EditStatus != EditStatus.ProviderOnly)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved by provider because EditStatus is {commitment.EditStatus}");
            }
        }

        private static void CheckAuthorization(long providerId, Commitment commitment)
        {
            if (commitment.ProviderId != providerId)
            {
                throw new UnauthorizedException($"Provider {providerId} not authorised to access commitment: {commitment.Id}, expected provider {commitment.ProviderId}");
            }
        }

        private static void CheckStateForApproval(Commitment commitment)
        {
            if (!commitment.ProviderCanApproveCommitment)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
            }
        }
    }
}
