using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandHandler : AsyncRequestHandler<UpdateCommitmentAgreementCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentAgreementCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipUpdateRules apprenticeshipUpdateRules, IApprenticeshipEvents apprenticeshipEvents)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (_apprenticeshipUpdateRules == null)
                throw new ArgumentNullException(nameof(_apprenticeshipUpdateRules));
            if (_apprenticeshipEvents == null)
                throw new ArgumentNullException(nameof(_apprenticeshipEvents));

            _commitmentRepository = commitmentRepository;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEvents = apprenticeshipEvents;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var commitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);

            CheckCommitmentStatus(commitment);
            CheckEditStatus(message, commitment);
            CheckAuthorization(message, commitment);

            var latestAction = (LastAction) message.LatestAction;

            // update apprenticeship agreement statuses
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                var hasChanged = false;

                //todo: extract status stuff outside loop and set all apprenticeships to same agreement status
                var newApprenticeshipAgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, message.Caller.CallerType, latestAction);
                var newApprenticeshipPaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(apprenticeship.PaymentStatus, newApprenticeshipAgreementStatus);

                if (apprenticeship.AgreementStatus != newApprenticeshipAgreementStatus)
                {
                    await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, apprenticeship.Id, newApprenticeshipAgreementStatus);
                    hasChanged = true;
                }

                if (apprenticeship.PaymentStatus != newApprenticeshipPaymentStatus)
                {
                    await _commitmentRepository.UpdateApprenticeshipStatus(message.CommitmentId, apprenticeship.Id, newApprenticeshipPaymentStatus);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    var updatedApprenticeship = await _commitmentRepository.GetApprenticeship(apprenticeship.Id);
                    await _apprenticeshipEvents.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED");
                }
            }

            var updatedCommitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);
            var areAnyApprenticeshipsPendingAgreement = updatedCommitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);

            // update commitment statuses
            await _commitmentRepository.UpdateCommitmentStatus(message.CommitmentId, _apprenticeshipUpdateRules.DetermineNewEditStatus(updatedCommitment.EditStatus, message.Caller.CallerType, areAnyApprenticeshipsPendingAgreement, updatedCommitment.Apprenticeships.Count));
            await _commitmentRepository.UpdateCommitmentStatus(message.CommitmentId, _apprenticeshipUpdateRules.DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement));
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

        private static string BuildInfoMessage(UpdateCommitmentAgreementCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called UpdateCommitmentAgreement for commitment {cmd.CommitmentId} with agreement status: {cmd.LatestAction}";
        }
    }
}
