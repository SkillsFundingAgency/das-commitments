using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CohortApproval.EmployerApproveCohort
{
    public sealed class EmployerApproveCohortCommandHandler : AsyncRequestHandler<EmployerApproveCohortCommand>
    {
        private readonly AbstractValidator<EmployerApproveCohortCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ICommitmentsLogger _logger;
        private readonly IFeatureToggleService _featureToggleService;
        private readonly IEmployerAccountsService _employerAccountsService;
        private readonly HistoryService _historyService;
        private readonly CohortApprovalService _cohortApprovalService;

        public EmployerApproveCohortCommandHandler(AbstractValidator<EmployerApproveCohortCommand> validator,
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules,
            ICurrentDateTime currentDateTime,
            IHistoryRepository historyRepository,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher,
            IMediator mediator,
            IMessagePublisher messagePublisher,
            ICommitmentsLogger logger,
            IApprenticeshipInfoService apprenticeshipInfoService,
            IFeatureToggleService featureToggleService,
            IEmployerAccountsService employerAccountsService,
            IV2EventsPublisher v2EventsPublisher = null)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _featureToggleService = featureToggleService;
            _employerAccountsService = employerAccountsService;
            _historyService = new HistoryService(historyRepository);
            
            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository,
                overlapRules,
                currentDateTime,
                commitmentRepository,
                apprenticeshipEventsList,
                apprenticeshipEventsPublisher,
                mediator,
                _logger,
                apprenticeshipInfoService, 
                v2EventsPublisher);
        }

        protected override async Task HandleCore(EmployerApproveCohortCommand message)
        {
            _validator.ValidateAndThrow(message);

            var commitment = await GetCommitment(message.CommitmentId);
            await CheckCommitmentCanBeApproved(commitment, message.Caller.Id);

            var haveBothPartiesApproved = HaveBothPartiesApproved(commitment);
            var newAgreementStatus = DetermineNewAgreementStatus(haveBothPartiesApproved);

            await UpdateCommitment(commitment, haveBothPartiesApproved, message.UserId, message.LastUpdatedByName,
                message.LastUpdatedByEmail, message.Message);

            await _cohortApprovalService.UpdateApprenticeships(commitment, haveBothPartiesApproved, newAgreementStatus);

            if (haveBothPartiesApproved)
            {
                if (commitment.HasTransferSenderAssigned)
                {
                    await _cohortApprovalService.CreateTransferRequest(commitment, _messagePublisher);
                }

                await PublishApprovedMessage(commitment);
            }

            await _cohortApprovalService.PublishApprenticeshipEvents(commitment, haveBothPartiesApproved);

            if (haveBothPartiesApproved && !commitment.HasTransferSenderAssigned)
            {
                await _cohortApprovalService.ReorderPayments(commitment.EmployerAccountId);
            }
        }

        private async Task PublishApprovedMessage(Commitment commitment)
        {
            await _messagePublisher.PublishAsync(new CohortApprovedByEmployer(commitment.EmployerAccountId,
                commitment.ProviderId.Value, commitment.Id));
        }

        private static AgreementStatus DetermineNewAgreementStatus(bool haveBothPartiesApproved)
        {
            var newAgreementStatus = haveBothPartiesApproved ? AgreementStatus.BothAgreed : AgreementStatus.EmployerAgreed;
            return newAgreementStatus;
        }

        private bool HaveBothPartiesApproved(Commitment commitment)
        {
            var currentAgreementStatus = _cohortApprovalService.GetCurrentAgreementStatus(commitment);
            return currentAgreementStatus == AgreementStatus.ProviderAgreed;
        }

        private async Task UpdateCommitment(Commitment commitment, bool haveBothPartiesApproved, string userId, string lastUpdatedByName, string lastUpdatedByEmail, string message)
        {
            var updatedEditStatus = DetermineNewEditStatus(haveBothPartiesApproved);
            var changeType = _cohortApprovalService.DetermineHistoryChangeType(haveBothPartiesApproved);
            _historyService.TrackUpdate(commitment, changeType.ToString(), commitment.Id, null, CallerType.Employer, userId, commitment.ProviderId, commitment.EmployerAccountId, lastUpdatedByName);

            commitment.EditStatus = updatedEditStatus;
            commitment.LastAction = LastAction.Approve;
            commitment.CommitmentStatus = CommitmentStatus.Active;
            commitment.LastUpdatedByEmployerEmail = lastUpdatedByEmail;
            commitment.LastUpdatedByEmployerName = lastUpdatedByName;
            commitment.TransferApprovalStatus = null;

            if (_featureToggleService.IsEnabled("ManageReservations") && haveBothPartiesApproved && !commitment.HasTransferSenderAssigned)
            {
                var account = await _employerAccountsService.GetAccount(commitment.EmployerAccountId);

                commitment.ApprenticeshipEmployerTypeOnApproval = account.ApprenticeshipEmployerType;
            }
            
            await Task.WhenAll(
                _cohortApprovalService.AddMessageToCommitment(commitment, lastUpdatedByName, message, CallerType.Employer),
                _commitmentRepository.UpdateCommitment(commitment), 
                _historyService.Save()
            );
        }

        internal EditStatus DetermineNewEditStatus(bool isFinalApproval)
        {
            return isFinalApproval ? EditStatus.Both : EditStatus.ProviderOnly;
        }

        private async Task CheckCommitmentCanBeApproved(Commitment commitment, long callerEmployerAccountId)
        {
            _cohortApprovalService.CheckCommitmentStatus(commitment);
            CheckEditStatus(commitment);
            CheckAuthorization(callerEmployerAccountId, commitment);
            CheckStateForApproval(commitment);
            await _cohortApprovalService.CheckOverlaps(commitment);
        }

        private async Task<Commitment> GetCommitment(long commitmentId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(commitmentId);
            return commitment;
        }

        private void CheckEditStatus(Commitment commitment)
        {
            if (commitment.EditStatus != EditStatus.EmployerOnly)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved by employer because EditStatus is {commitment.EditStatus}");
            }
        }

        private static void CheckAuthorization(long employerAccountId, Commitment commitment)
        {
            if (commitment.EmployerAccountId != employerAccountId)
            {
                throw new UnauthorizedException($"Employer {employerAccountId} not authorised to access commitment: {commitment.Id}, expected employer {commitment.EmployerAccountId}");
            }
        }

        private static void CheckStateForApproval(Commitment commitment)
        {
            if (!commitment.EmployerCanApproveCommitment)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
            }
        }
    }
}
