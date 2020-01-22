using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;

namespace SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort
{
    public sealed class ProviderApproveCohortCommandHandler : AsyncRequestHandler<ProviderApproveCohortCommand>
    {
        private readonly AbstractValidator<ProviderApproveCohortCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IV2EventsPublisher _v2EventsPublisher;
        private readonly CohortApprovalService _cohortApprovalService;

        public ProviderApproveCohortCommandHandler(AbstractValidator<ProviderApproveCohortCommand> validator,
            ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules,
            ICurrentDateTime currentDateTime,
            IHistoryRepository historyRepository,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher,
            IMediator mediator,
            ICommitmentsLogger logger,
            IApprenticeshipInfoService apprenticeshipInfoService,
            IV2EventsPublisher v2EventsPublisher = null)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _logger = logger;
            _v2EventsPublisher = v2EventsPublisher;
            new HistoryService(historyRepository);
            _cohortApprovalService = new CohortApprovalService(apprenticeshipRepository, overlapRules, currentDateTime, commitmentRepository, apprenticeshipEventsList, apprenticeshipEventsPublisher, mediator, _logger, apprenticeshipInfoService, v2EventsPublisher);
        }

        protected override async Task HandleCore(ProviderApproveCohortCommand message)
        {
            _validator.ValidateAndThrow(message);

            var commitment = await GetCommitment(message.CommitmentId);
            await CheckCommitmentCanBeApproved(commitment, message.Caller.Id);
            
            var userInfo = new UserInfo
            {
                UserDisplayName = message.LastUpdatedByName,
                UserEmail = message.LastUpdatedByEmail,
                UserId = message.UserId
            };

            await _v2EventsPublisher.SendProviderApproveCohortCommand(message.CommitmentId, userInfo);
        }

       
        private async Task<Commitment> GetCommitment(long commitmentId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(commitmentId);
            return commitment;
        }

        private async Task CheckCommitmentCanBeApproved(Commitment commitment, long callerEmployerAccountId)
        {
            _cohortApprovalService.CheckCommitmentStatus(commitment);
            CheckEditStatus(commitment);
            CheckAuthorization(callerEmployerAccountId, commitment);
            CheckStateForApproval(commitment);
            await _cohortApprovalService.CheckOverlaps(commitment);
        }

        private static void CheckEditStatus(Commitment commitment)
        {
            if (commitment.EditStatus != EditStatus.ProviderOnly)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved by provider because EditStatus is {commitment.EditStatus}");
            }
        }

        private static void CheckAuthorization(long providerId, Commitment commitment)
        {
            if (commitment.ProviderId != providerId)
            {
                throw new UnauthorizedException($"Provider {providerId} not authorised to access commitment: {commitment.Id}, expected provider {commitment.ProviderId}");
            }
        }

        private static void CheckStateForApproval(Commitment commitment)
        {
            if (!commitment.ProviderCanApproveCommitment)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
            }
        }
    }
}
