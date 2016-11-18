using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<UpdateApprenticeshipCommand> _validator;
        private readonly IEventsApi _eventsApi;
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;

        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateApprenticeshipCommand> validator, IEventsApi eventsApi, IApprenticeshipUpdateRules apprenticeshipUpdateRules)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _eventsApi = eventsApi;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);
            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            CheckAuthorization(message, commitment);
            CheckCommitmentStatus(message, commitment);
            CheckEditStatus(message, commitment);
            CheckPaymentStatus(apprenticeship);

            var updatedApprenticeship = MapFrom(message.Apprenticeship, message);

            var doChangesRequireAgreement = _apprenticeshipUpdateRules.DetermineWhetherChangeRequireAgreement(apprenticeship, updatedApprenticeship);

            updatedApprenticeship.AgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, message.Caller.CallerType, doChangesRequireAgreement);
            updatedApprenticeship.PaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(apprenticeship.PaymentStatus, doChangesRequireAgreement);

            await _commitmentRepository.UpdateApprenticeship(updatedApprenticeship, message.Caller);

            await PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED");
        }

        private static void CheckCommitmentStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Apprenticeship {message.ApprenticeshipId} in commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckPaymentStatus(Apprenticeship apprenticeship)
        {
            var allowedPaymentStatusesForUpdating = new[] {PaymentStatus.Active, PaymentStatus.PendingApproval, PaymentStatus.Paused};

            if (!allowedPaymentStatusesForUpdating.Contains(apprenticeship.PaymentStatus))
                throw new UnauthorizedException($"Apprenticeship {apprenticeship.Id} cannot be updated when payment status is {apprenticeship.PaymentStatus}");
        }

        private static void CheckAuthorization(UpdateApprenticeshipCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static Apprenticeship MapFrom(Api.Types.Apprenticeship apprenticeship, UpdateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Apprenticeship
            {
                Id = message.ApprenticeshipId, FirstName = apprenticeship.FirstName, LastName = apprenticeship.LastName, DateOfBirth = apprenticeship.DateOfBirth, NINumber = apprenticeship.NINumber, ULN = apprenticeship.ULN, CommitmentId = message.CommitmentId, PaymentStatus = (PaymentStatus) apprenticeship.PaymentStatus, AgreementStatus = (AgreementStatus) apprenticeship.AgreementStatus, TrainingType = (TrainingType) apprenticeship.TrainingType, TrainingCode = apprenticeship.TrainingCode, TrainingName = apprenticeship.TrainingName, Cost = apprenticeship.Cost, StartDate = apprenticeship.StartDate, EndDate = apprenticeship.EndDate, EmployerRef = apprenticeship.EmployerRef, ProviderRef = apprenticeship.ProviderRef
            };

            return domainApprenticeship;
        }

        private async Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            var apprenticeshipEvent = new ApprenticeshipEvent
            {
                AgreementStatus = apprenticeship.AgreementStatus.ToString(),
                ApprenticeshipId = apprenticeship.Id,
                EmployerAccountId = commitment.EmployerAccountId.ToString(),
                LearnerId = apprenticeship.ULN ?? "NULL",
                TrainingId = apprenticeship.TrainingCode,
                Event = @event,
                PaymentStatus = apprenticeship.PaymentStatus.ToString(),
                ProviderId = commitment.ProviderId.ToString(),
                TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue,
                TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard
            };

            //todo: publish event (temporarily disabled)
            //await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }

        private static string BuildInfoMessage(UpdateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called UpdateApprenticeshipCommand";
        }
    }
}
