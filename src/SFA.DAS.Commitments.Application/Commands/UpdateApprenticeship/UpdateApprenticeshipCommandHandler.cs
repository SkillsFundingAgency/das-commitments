using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
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

        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateApprenticeshipCommand> validator, IEventsApi eventsApi)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _eventsApi = eventsApi;
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckEditStatus(message, commitment);
            CheckAuthorization(message, commitment);

            var submittedApprenticeship = MapFrom(message.Apprenticeship, message);
            var existingApprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            var hasChanged = false;
            var doChangesRequireAgreement = DetermineWhetherChangeRequireAgreement(existingApprenticeship, submittedApprenticeship);
            submittedApprenticeship.AgreementStatus = DetermineNewAgreementStatus(existingApprenticeship.AgreementStatus, message.Caller.CallerType, doChangesRequireAgreement);
            submittedApprenticeship.PaymentStatus = DetermineNewPaymentStatus(existingApprenticeship.PaymentStatus, submittedApprenticeship.AgreementStatus);

            if (existingApprenticeship.AgreementStatus != submittedApprenticeship.AgreementStatus)
            {
                hasChanged = true;
            }

            if (existingApprenticeship.PaymentStatus != submittedApprenticeship.PaymentStatus)
            {
                hasChanged = true;
            }

            if (hasChanged)
            {
                await _commitmentRepository.UpdateApprenticeship(submittedApprenticeship, message.Caller);

                //todo: publish event (temporarily disabled)
                //var updatedApprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);
                //await PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED");
            }
        }

        private static AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, bool doChangesRequireAgreement)
        {
            throw new NotImplementedException();
        }

        private static PaymentStatus DetermineNewPaymentStatus(PaymentStatus paymentStatus, AgreementStatus newAgreementStatus)
        {
            throw new NotImplementedException();
        }

        private static bool DetermineWhetherChangeRequireAgreement(Apprenticeship existingApprenticeship, Apprenticeship submittedApprenticeship)
        {
            throw new NotImplementedException();
        }

        private static Apprenticeship MapFrom(Api.Types.Apprenticeship apprenticeship, UpdateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Apprenticeship
            {
                Id = message.ApprenticeshipId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                PaymentStatus = (PaymentStatus)apprenticeship.PaymentStatus,
                AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                TrainingType = (TrainingType)apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate,
                EmployerRef = apprenticeship.EmployerRef,
                ProviderRef = apprenticeship.ProviderRef
            };

            return domainApprenticeship;
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

        private async Task PublishEvent(Commitment commitment, Apprenticeship apprentice, string @event)
        {
            var apprenticeshipEvent = new ApprenticeshipEvent
            {
                AgreementStatus = apprentice.AgreementStatus.ToString(),
                ApprenticeshipId = apprentice.Id,
                EmployerAccountId = commitment.EmployerAccountId.ToString(),
                LearnerId = apprentice.ULN ?? "NULL",
                TrainingId = apprentice.TrainingCode,
                Event = @event,
                PaymentStatus = apprentice.PaymentStatus.ToString(),
                ProviderId = commitment.ProviderId.ToString(),
                TrainingEndDate = apprentice.EndDate ?? DateTime.MaxValue,
                TrainingStartDate = apprentice.StartDate ?? DateTime.MaxValue,
                TrainingTotalCost = apprentice.Cost ?? Decimal.MinValue,
                TrainingType =  apprentice.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard

            };

            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }


        private static string BuildInfoMessage(UpdateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called UpdateApprenticeshipCommand";
        }
    }
}
