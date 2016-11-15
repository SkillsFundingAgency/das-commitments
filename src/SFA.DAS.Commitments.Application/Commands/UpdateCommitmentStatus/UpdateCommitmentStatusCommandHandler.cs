using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusCommandHandler : AsyncRequestHandler<UpdateCommitmentStatusCommand>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly AbstractValidator<UpdateCommitmentStatusCommand> _validator;
        private readonly IEventsApi _eventsApi;
        private readonly ICommitmentRepository _commitmentRepository;

        public UpdateCommitmentStatusCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateCommitmentStatusCommand> validator, IEventsApi eventsApi)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _eventsApi = eventsApi;
        }

        protected override async Task HandleCore(UpdateCommitmentStatusCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            if (message.CommitmentStatus.HasValue && commitment.CommitmentStatus != (CommitmentStatus) message.CommitmentStatus.Value)
            {
                await _commitmentRepository.UpdateStatus(message.CommitmentId, (CommitmentStatus)message.CommitmentStatus);
                await PublishEvent(commitment, "COMMITMENT-STATUS-UPDATED");
            }
        }

        public async Task PublishEvent(Commitment commitment, string @event)
        {
            foreach (var a in commitment.Apprenticeships)
            {
        
                var apprenticeshipEvent = new ApprenticeshipEvent
                {
                    AgreementStatus = a.AgreementStatus.ToString(),
                    ApprenticeshipId = a.Id,
                    EmployerAccountId = commitment.EmployerAccountId.ToString(),
                    LearnerId = a.ULN ?? "NULL",
                    TrainingId = a.TrainingCode,
                    Event = @event,
                    PaymentStatus = a.PaymentStatus.ToString(),
                    ProviderId = commitment.ProviderId.ToString(),
                    TrainingEndDate = a.EndDate ?? DateTime.MaxValue,
                    TrainingStartDate = a.StartDate ?? DateTime.MaxValue,
                    TrainingTotalCost = a.Cost ?? Decimal.MinValue,
                    TrainingType =  a.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard

                };

                await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
            }
        }

        private static void CheckAuthorization(UpdateCommitmentStatusCommand message, Commitment commitment)
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

        private string BuildInfoMessage(UpdateCommitmentStatusCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called CreateApprenticeshipCommand";
        }
    }
}
