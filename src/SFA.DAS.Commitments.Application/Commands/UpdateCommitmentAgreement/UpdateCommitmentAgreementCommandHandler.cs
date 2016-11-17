using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandHandler : AsyncRequestHandler<UpdateCommitmentAgreementCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventsApi _eventsApi;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentAgreementCommandHandler(ICommitmentRepository commitmentRepository, IEventsApi eventsApi)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            _commitmentRepository = commitmentRepository;
            _eventsApi = eventsApi;
        }

        protected override async Task HandleCore(UpdateCommitmentAgreementCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckEditStatus(message, commitment);
            CheckAuthorization(message, commitment);

            var newAgreementStatus = (AgreementStatus) message.AgreementStatus;

            // update apprenticeship agreement statuses
            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                var hasChanged = false;
                var newApprenticeshipAgreementStatus = DetermineNewAgreementStatus(apprenticeship.AgreementStatus, message.Caller.CallerType, newAgreementStatus);
                var newApprenticeshipPaymentStatus = DetermineNewPaymentStatus(apprenticeship.PaymentStatus, newApprenticeshipAgreementStatus);

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
                    //todo: publish event (temporarily disabled)
                    //var updatedApprenticeship = await _commitmentRepository.GetApprenticeship(apprenticeship.Id);
                    //await PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED");
                }
            }

            //todo: reduce db calls
            var updatedCommitment = await _commitmentRepository.GetById(message.CommitmentId);
            var areAnyApprenticeshipsPendingAgreement = updatedCommitment.Apprenticeships.Any(a => a.AgreementStatus != AgreementStatus.BothAgreed);

            // update commitment statuses
            await _commitmentRepository.UpdateCommitmentStatus(message.CommitmentId, DetermineNewEditStatus(message.Caller.CallerType, areAnyApprenticeshipsPendingAgreement));
            await _commitmentRepository.UpdateCommitmentStatus(message.CommitmentId, DetermineNewCommmitmentStatus(areAnyApprenticeshipsPendingAgreement));
        }

        private static CommitmentStatus DetermineNewCommmitmentStatus(bool areAnyApprenticeshipsPendingAgreement)
        {
            //todo: commitment status will be set to "deleted" if all apprenticeships are agreed (after private beta wave 2b.1)
            return areAnyApprenticeshipsPendingAgreement ? CommitmentStatus.Active : CommitmentStatus.Active;
        }

        private PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, AgreementStatus newApprenticeshipAgreementStatus)
        {
            switch (currentPaymentStatus)
            {
                case PaymentStatus.PendingApproval:
                case PaymentStatus.Active:
                case PaymentStatus.Paused:
                    return newApprenticeshipAgreementStatus == AgreementStatus.BothAgreed ? PaymentStatus.Active : PaymentStatus.PendingApproval;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentPaymentStatus), currentPaymentStatus, null);
            }
        }

        private static EditStatus DetermineNewEditStatus(CallerType caller, bool areAnyApprenticeshipsPendingAgreement)
        {
            //todo: extract and unit test
            if (areAnyApprenticeshipsPendingAgreement)
                return caller == CallerType.Provider ? EditStatus.EmployerOnly : EditStatus.ProviderOnly;

            return EditStatus.Both;
        }

        private static AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, AgreementStatus newAgreementStatus)
        {
            //todo: extract and unit test
            switch (newAgreementStatus)
            {
                case AgreementStatus.NotAgreed:
                    return AgreementStatus.NotAgreed;

                case AgreementStatus.EmployerAgreed:
                case AgreementStatus.ProviderAgreed:
                    switch (currentAgreementStatus)
                    {
                        case AgreementStatus.NotAgreed:
                        case AgreementStatus.BothAgreed:
                            return newAgreementStatus;

                        case AgreementStatus.EmployerAgreed:
                            return caller == CallerType.Employer ? AgreementStatus.EmployerAgreed : AgreementStatus.BothAgreed;

                        case AgreementStatus.ProviderAgreed:
                            return caller == CallerType.Employer ? AgreementStatus.BothAgreed : AgreementStatus.ProviderAgreed;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(currentAgreementStatus), currentAgreementStatus, null);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(newAgreementStatus), newAgreementStatus, null);
            }
        }

        private static void CheckEditStatus(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider unauthorized to edit commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer unauthorized to edit commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckAuthorization(UpdateCommitmentAgreementCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static string BuildInfoMessage(UpdateCommitmentAgreementCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called UpdateCommitmentAgreement for commitment {cmd.CommitmentId} with agreement status: {cmd.AgreementStatus}";
        }

        private async Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            var apprenticeshipEvent = new ApprenticeshipEvent
            {
                AgreementStatus = apprenticeship.AgreementStatus.ToString(), ApprenticeshipId = apprenticeship.Id, EmployerAccountId = commitment.EmployerAccountId.ToString(), LearnerId = apprenticeship.ULN ?? "NULL", TrainingId = apprenticeship.TrainingCode, Event = @event, PaymentStatus = apprenticeship.PaymentStatus.ToString(), ProviderId = commitment.ProviderId.ToString(), TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue, TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue, TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue, TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard
            };

            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }
    }
}
