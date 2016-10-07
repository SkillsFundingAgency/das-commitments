using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateApprenticeshipCommand> validator)
        {
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

            return apprenticeshipId;
        }

        private Domain.Apprenticeship MapFrom(Apprenticeship apprenticeship, CreateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Domain.Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                Status = Domain.ApprenticeshipStatus.ReadyForApproval,
                AgreementStatus = (Domain.AgreementStatus)apprenticeship.AgreementStatus,
                TrainingId = apprenticeship.TrainingId,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate
            };

            return domainApprenticeship;
        }

        private static void CheckAuthorization(CreateApprenticeshipCommand message, Domain.Commitment commitment)
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
    }
}
