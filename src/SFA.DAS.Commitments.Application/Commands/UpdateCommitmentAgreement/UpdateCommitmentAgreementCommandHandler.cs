using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    //todo: add test for UpdateCommitmentAgreementCommandHandler various scenarios
    public sealed class UpdateCommitmentAgreementCommandHandler : AsyncRequestHandler<UpdateCommitmentAgreementCommand>
    {
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentRepository _commitmentRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        private readonly ICommitmentsLogger _logger;
        private readonly IMediator _mediator;
        private readonly AbstractValidator<UpdateCommitmentAgreementCommand> _validator;

        public UpdateCommitmentAgreementCommandHandler(
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipUpdateRules apprenticeshipUpdateRules, 
            IApprenticeshipEvents apprenticeshipEvents, 
            ICommitmentsLogger logger, 
            IMediator mediator,
            AbstractValidator<UpdateCommitmentAgreementCommand> validator)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(apprenticeshipRepository));
            if (apprenticeshipUpdateRules == null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateRules));
            if (apprenticeshipEvents == null)
                throw new ArgumentNullException(nameof(apprenticeshipEvents));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _mediator = mediator;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand command)
        {
            _validator.ValidateAndThrow(command);

            LogMessage(command);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);

            CheckCommitmentStatus(commitment);
            CheckEditStatus(command, commitment);
            CheckAuthorization(command, commitment);

            if (command.LatestAction == Api.Types.Commitment.Types.LastAction.Approve)
            {
                CheckStateForApproval(commitment, command.Caller);

                var overlaps = await GetOverlappingApprenticeships(commitment);
                if (overlaps.Data.Any())
                {
                    throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
                }
            }

            var latestAction = (LastAction) command.LatestAction;

            // update apprenticeship agreement statuses
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                var hasChanged = false;

                //todo: extract status stuff outside loop and set all apprenticeships to same agreement status?
                var newApprenticeshipAgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, command.Caller.CallerType, latestAction);
                var newApprenticeshipPaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(apprenticeship.PaymentStatus, newApprenticeshipAgreementStatus);

                if (apprenticeship.AgreementStatus != newApprenticeshipAgreementStatus)
                {
                    await _apprenticeshipRepository.UpdateApprenticeshipStatus(command.CommitmentId, apprenticeship.Id, newApprenticeshipAgreementStatus);
                    hasChanged = true;
                }

                if (apprenticeship.PaymentStatus != newApprenticeshipPaymentStatus)
                {
                    await _apprenticeshipRepository.UpdateApprenticeshipStatus(command.CommitmentId, apprenticeship.Id, newApprenticeshipPaymentStatus);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    var updatedApprenticeship = await _apprenticeshipRepository.GetApprenticeship(apprenticeship.Id);
                    await _apprenticeshipEvents.PublishEvent(updatedApprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED");
                }
            }

            var updatedCommitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var areAnyApprenticeshipsPendingAgreement = updatedCommitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);

            // update commitment statuses
            var newEditStatus = _apprenticeshipUpdateRules.DetermineNewEditStatus(updatedCommitment.EditStatus, command.Caller.CallerType, areAnyApprenticeshipsPendingAgreement, updatedCommitment.Apprenticeships.Count, latestAction);
            var newCommitmentStatus = _apprenticeshipUpdateRules.DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement);

            var lastUpdateAction = new LastUpdateAction
                         {
                             LastUpdaterName = command.LastUpdatedByName,
                             LastUpdaterEmail = command.LastUpdatedByEmail,
                             Caller = command.Caller,
                             LastAction = latestAction,
                             UserId = command.UserId
                         };
            await _commitmentRepository.UpdateCommitment(command.CommitmentId, newCommitmentStatus, newEditStatus, lastUpdateAction);

            // recalculate payment order for all the employer account's apprenticeships if necessary
            await SetPaymentOrderIfNeeded(command.Caller, commitment.EmployerAccountId, commitment.Apprenticeships.Count, latestAction, areAnyApprenticeshipsPendingAgreement);
        }

        private void LogMessage(UpdateCommitmentAgreementCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called UpdateCommitmentAgreement for commitment {command.CommitmentId} with agreement status: {command.LatestAction}";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckAuthorization(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckStateForApproval(Commitment commitment, Caller caller)
        {
            var canBeApprovedByParty = caller.CallerType == CallerType.Employer
                ? commitment.EmployerCanApproveCommitment
                : commitment.ProviderCanApproveCommitment;

            if (!canBeApprovedByParty)
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
        }

        private async Task SetPaymentOrderIfNeeded(Caller caller, long employerAccountId, int apprenticeshipsCount, LastAction latestAction, bool areAnyApprenticeshipsPendingAgreement)
        {
            if (latestAction == LastAction.Approve && apprenticeshipsCount > 0 && !areAnyApprenticeshipsPendingAgreement)
            {
                await _mediator.SendAsync(new SetPaymentOrderCommand {AccountId = employerAccountId});
            }
        }

        private async Task<GetOverlappingApprenticeshipsResponse> GetOverlappingApprenticeships(Commitment commitment)
        {
            var overlapRequests = 
                commitment.Apprenticeships
                .Where(x => !string.IsNullOrWhiteSpace(x.ULN) && x.StartDate.HasValue && x.EndDate.HasValue)
                .Select(apprenticeship => 
                    new ApprenticeshipOverlapValidationRequest
                        {
                            Uln = apprenticeship.ULN,
                            StartDate = apprenticeship.StartDate.Value,
                            EndDate = apprenticeship.EndDate.Value
                        })
                .ToList();

            var response = await _mediator.SendAsync(new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = overlapRequests
            });

            return response;
        }
    }
}
