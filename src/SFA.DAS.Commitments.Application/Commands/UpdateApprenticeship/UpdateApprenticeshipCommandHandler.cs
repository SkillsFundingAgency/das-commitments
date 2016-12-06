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
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    //todo: add test for UpdateApprenticeshipCommandHandler various scenarios

    public sealed class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<UpdateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipUpdateRules _apprenticeshipUpdateRules;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;
        private readonly ICommitmentsLogger _logger;

        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateApprenticeshipCommand> validator, IApprenticeshipUpdateRules apprenticeshipUpdateRules, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _apprenticeshipUpdateRules = apprenticeshipUpdateRules;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
        }


        protected override async Task HandleCore(UpdateApprenticeshipCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            var apprenticeship = await _commitmentRepository.GetApprenticeship(command.ApprenticeshipId);

            CheckAuthorization(command, commitment);
            CheckCommitmentStatus(command, commitment);
            CheckEditStatus(command, commitment);
            CheckPaymentStatus(apprenticeship);

            var updatedApprenticeship = MapFrom(command.Apprenticeship, command);

            var doChangesRequireAgreement = _apprenticeshipUpdateRules.DetermineWhetherChangeRequireAgreement(apprenticeship, updatedApprenticeship);

            updatedApprenticeship.AgreementStatus = _apprenticeshipUpdateRules.DetermineNewAgreementStatus(apprenticeship.AgreementStatus, command.Caller.CallerType, doChangesRequireAgreement);
            updatedApprenticeship.PaymentStatus = _apprenticeshipUpdateRules.DetermineNewPaymentStatus(apprenticeship.PaymentStatus, doChangesRequireAgreement);

            await _commitmentRepository.UpdateApprenticeship(updatedApprenticeship, command.Caller);

            await _apprenticeshipEvents.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED");
        }

        private void LogMessage(UpdateApprenticeshipCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called UpdateApprenticeshipCommand";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, apprenticeshipId: command.ApprenticeshipId);
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to edit apprenticeship {message.ApprenticeshipId} in commitment {message.CommitmentId}");
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
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
    }
}
