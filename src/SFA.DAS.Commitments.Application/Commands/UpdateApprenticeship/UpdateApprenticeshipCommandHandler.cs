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

            CheckAuthorization(message, commitment);

            await _commitmentRepository.UpdateApprenticeship(MapFrom(message.Apprenticeship, message), message.Caller);

            await PublishEvent(commitment, MapFrom(message.Apprenticeship, message), "APPRENTICESHIP-UPDATED");
        }

        private Apprenticeship MapFrom(Api.Types.Apprenticeship apprenticeship, UpdateApprenticeshipCommand message)
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

        public async Task PublishEvent(Commitment commitment, Apprenticeship apprentice, string @event)
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


        private string BuildInfoMessage(UpdateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called UpdateApprenticeshipCommand";
        }
    }
}
