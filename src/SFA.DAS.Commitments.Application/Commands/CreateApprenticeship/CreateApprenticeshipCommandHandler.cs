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
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventsApi _eventsApi;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;

        public CreateApprenticeshipCommandHandler(IEventsApi eventsApi,ICommitmentRepository commitmentRepository, AbstractValidator<CreateApprenticeshipCommand> validator)
        {
            _eventsApi = eventsApi;
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<long> Handle(CreateApprenticeshipCommand message)
        {
            Logger.Info(BuildInfoMessage(message));

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            CheckAuthorization(message, commitment);

            var apprenticeshipId = await _commitmentRepository.CreateApprenticeship(MapFrom(message.Apprenticeship, message));

            message.Apprenticeship.Id = apprenticeshipId;

            // Not pushing to events API for 2b.1
            // await PublishEvent(commitment, MapFrom(message.Apprenticeship, message), "APPRENTICESHIP-CREATED");

            return apprenticeshipId;
        }

        private Domain.Entities.Apprenticeship MapFrom(Apprenticeship apprenticeship, CreateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Domain.Entities.Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                PaymentStatus = PaymentStatus.PendingApproval,
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

        private static void CheckAuthorization(CreateApprenticeshipCommand message, Commitment commitment)
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

        private string BuildInfoMessage(CreateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called CreateApprenticeshipCommand";
        }

        public async Task PublishEvent(Commitment commitment, Domain.Entities.Apprenticeship apprentice, string @event)
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
                TrainingType = apprentice.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard
            };

            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }

    }
}
