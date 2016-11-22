using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;
        private readonly IApprenticeshipEvents _apprenticeshipEvents;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateApprenticeshipCommand> validator, IApprenticeshipEvents apprenticeshipEvents)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
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

            await _apprenticeshipEvents.PublishEvent(commitment, MapFrom(message.Apprenticeship, message), "APPRENTICESHIP-CREATED");

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
                AgreementStatus = (Domain.Entities.AgreementStatus) apprenticeship.AgreementStatus,
                TrainingType = (Domain.Entities.TrainingType) apprenticeship.TrainingType,
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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private string BuildInfoMessage(CreateApprenticeshipCommand cmd)
        {
            return $"{cmd.Caller.CallerType}: {cmd.Caller.Id} has called CreateApprenticeshipCommand";
        }
    }
}
