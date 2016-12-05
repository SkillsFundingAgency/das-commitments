using System;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Logging;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class EmployerOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;

        public EmployerOrchestrator(IMediator mediator, ILog logger)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _mediator = mediator;
            _logger = logger;
        }

        public async Task<GetCommitmentsResponse> GetCommitments(long accountId)
        {
            _logger.Info($"Getting commitments for employer account {accountId}", new CommitmentsLogEntry { AccountId = accountId });

            return await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });
        }

        public async Task<GetCommitmentResponse> GetCommitment(long accountId, long commitmentId)
        {
            _logger.Info($"Getting commitment {commitmentId} for employer account {accountId}", new CommitmentsLogEntry { AccountId = accountId, CommitmentId = commitmentId });

            return await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId
            });
        }

        public async Task<long> CreateCommitment(long accountId, Commitment commitment)
        {
            _logger.Info($"Creating commitment for employer account {accountId}");

            commitment.EmployerAccountId = accountId;

            return await _mediator.SendAsync(new CreateCommitmentCommand
            {
                Commitment = commitment
            });
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId)
        {
            _logger.Info($"Getting apprenticeships for employer account {accountId}");

            return await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long accountId, long apprenticeshipId)
        {
            _logger.Info($"Getting apprenticeship {apprenticeshipId} for employer account {accountId}");

            return await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId
            });
        }

        public async Task<long> CreateApprenticeship(long accountId, long commitmentId, Apprenticeship apprenticeship)
        {
            _logger.Info($"Creating apprenticeship for commitment {commitmentId} for employer account {accountId}");

            apprenticeship.CommitmentId = commitmentId;

            return await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            _logger.Info($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}");

            apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task PatchCommitment(long accountId, long commitmentId, LastAction latestAction)
        {
            _logger.Info($"Updating latest action to {latestAction} for commitment {commitmentId} for employer account {accountId}");

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                LatestAction = latestAction
            });
        }

        public async Task PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, PaymentStatus? paymentStatus)
        {
            _logger.Info($"Updating payment status to {paymentStatus} for commitment {commitmentId} for employer account {accountId}");

            await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand
            {
                AccountId = accountId,
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                PaymentStatus = paymentStatus
            });
        }
    }
}
