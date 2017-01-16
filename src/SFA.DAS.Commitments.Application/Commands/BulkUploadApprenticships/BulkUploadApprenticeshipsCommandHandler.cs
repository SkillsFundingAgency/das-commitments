using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommandHandler : AsyncRequestHandler<BulkUploadApprenticeshipsCommand>
    {
        private BulkUploadApprenticeshipsValidator _validator;
        private ICommitmentsLogger _logger;
        private ICommitmentRepository _commitmentRepository;
        private IApprenticeshipEvents _apprenticeshipEvents;

        public BulkUploadApprenticeshipsCommandHandler(ICommitmentRepository commitmentRepository, BulkUploadApprenticeshipsValidator validator, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (apprenticeshipEvents == null)
                throw new ArgumentNullException(nameof(apprenticeshipEvents));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
        }

        protected override async Task HandleCore(BulkUploadApprenticeshipsCommand command)
        {
            LogMessage(command);

            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            if (commitment == null)
                throw new ResourceNotFoundException($"Provider { command.Caller.Id } specified a non-existant Commitment { command.CommitmentId}");

            // TODO: This logic can be shared between handlers.
            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(commitment);

            var apprenticeships = command.Apprenticeships.Select(x => MapFrom(x, command));

            Stopwatch watch = Stopwatch.StartNew();
            var insertedApprenticeships = await _commitmentRepository.BulkUploadApprenticeships(command.CommitmentId, apprenticeships);
            _logger.Trace($"Bulk insert of {command.Apprenticeships.Count} apprentices into Db took {watch.ElapsedMilliseconds} milliseconds");

            // TODO: Need better way to publish all these events
            watch = Stopwatch.StartNew();
            await _apprenticeshipEvents.BulkPublishEvent(commitment, insertedApprenticeships, "APPRENTICESHIP-CREATED");
            //await Task.WhenAll(insertedApprenticeships.Select(a => _apprenticeshipEvents.PublishEvent(commitment, a, "APPRENTICESHIP-CREATED")));
            _logger.Trace($"Publishing {command.Apprenticeships.Count} apprenticeship-created events took {watch.ElapsedMilliseconds} milliseconds");
        }

        private Domain.Entities.Apprenticeship MapFrom(Api.Types.Apprenticeship apprenticeship, BulkUploadApprenticeshipsCommand message)
        {
            var domainApprenticeship = new Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                PaymentStatus = PaymentStatus.PendingApproval,
                AgreementStatus = AgreementStatus.NotAgreed,
                TrainingType = (TrainingType)apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate
            };

            SetCallerSpecificReference(domainApprenticeship, apprenticeship, message.Caller.CallerType);

            return domainApprenticeship;
        }

        private static void SetCallerSpecificReference(Apprenticeship domainApprenticeship, Api.Types.Apprenticeship apiApprenticeship, CallerType callerType)
        {
            if (callerType.IsEmployer())
                domainApprenticeship.EmployerRef = apiApprenticeship.EmployerRef;
            else
                domainApprenticeship.ProviderRef = apiApprenticeship.ProviderRef;
        }

        private void LogMessage(BulkUploadApprenticeshipsCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called BulkUploadApprenticeshipsCommand with {command.Apprenticeships?.Count ?? 0} apprenticeships";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private static void CheckAuthorization(BulkUploadApprenticeshipsCommand message, Commitment commitment)
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

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.New && commitment.CommitmentStatus != Domain.Entities.CommitmentStatus.Active)
                throw new InvalidOperationException($"Cannot add apprenticeship in commitment {commitment.Id} because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(BulkUploadApprenticeshipsCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to add apprenticeships to commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != Domain.Entities.EditStatus.Both && commitment.EditStatus != Domain.Entities.EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to add apprenticeship to commitment {message.CommitmentId}");
                    break;
            }
        }
    }
}
