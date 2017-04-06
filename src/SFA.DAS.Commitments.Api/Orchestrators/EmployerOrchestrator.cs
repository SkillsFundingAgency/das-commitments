using System;
using System.Threading.Tasks;
using System.Web.UI;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class EmployerOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public EmployerOrchestrator(IMediator mediator, ICommitmentsLogger logger)
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
            _logger.Info($"Getting commitments for employer account {accountId}", accountId: accountId);

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
            _logger.Info($"Getting commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

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

        public async Task<long> CreateCommitment(long accountId, Commitment.CommitmentRequest commitmentRequest)
        {
            _logger.Info($"Creating commitment for employer account {accountId}", accountId: accountId);

            commitmentRequest.Commitment.EmployerAccountId = accountId;

            return await _mediator.SendAsync(new CreateCommitmentCommand
            {
                Commitment = commitmentRequest.Commitment,
                UserId = commitmentRequest.UserId,
                CallerType = CallerType.Employer
            });
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId)
        {
            _logger.Info($"Getting apprenticeships for employer account {accountId}", accountId: accountId);

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
            _logger.Info($"Getting apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

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

        public async Task<long> CreateApprenticeship(long accountId, long commitmentId, Apprenticeship.ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Info($"Creating apprenticeship for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            return await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId
            });
        }

        public async Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship.ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Info($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId
            });
        }

        public async Task PatchCommitment(long accountId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Info($"Updating latest action to {submission.Action} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                LatestAction = submission.Action,
                LastUpdatedByName = submission.LastUpdatedByInfo.Name,
                LastUpdatedByEmail = submission.LastUpdatedByInfo.EmailAddress,
                UserId = submission.UserId
            });
        }

        public async Task PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission)
        {
            _logger.Info($"Updating payment status to {apprenticeshipSubmission.PaymentStatus} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand
            {
                AccountId = accountId,
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                PaymentStatus = apprenticeshipSubmission.PaymentStatus,
                UserId = apprenticeshipSubmission.UserId
            });
        }

        public async Task DeleteApprenticeship(long accountId, long apprenticeshipId, string userId)
        {
            _logger.Info($"Deleting apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new DeleteApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId,
                UserId = userId
            });
        }

        public async Task DeleteCommitment(long accountId, long commitmentId, string userId)
        {
            _logger.Info($"Deleting commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new DeleteCommitmentCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                UserId = userId
            });
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId)
        {
            _logger.Info($"Getting pending update for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: apprenticeshipId );

            var response = await _mediator.SendAsync(new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }

        public async Task CreateApprenticeshipUpdate(long accountId, Apprenticeship.ApprenticeshipUpdateRequest updateRequest)
        {
            _logger.Info($"Creating update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);

            await _mediator.SendAsync(new CreateApprenticeshipUpdateCommand
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Employer,
                        Id = accountId
                    },
                    ApprenticeshipUpdate = updateRequest.ApprenticeshipUpdate
            });
        }

        public async Task PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipUpdateSubmission submission)
        {
            _logger.Info($"Patching update for apprenticeship {apprenticeshipId} for employer account {accountId} with status {submission.UpdateStatus}", accountId, apprenticeshipId: apprenticeshipId);

            var command = 
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = apprenticeshipId,
                    Caller = new Caller(accountId, CallerType.Employer),
                    UserId = submission.UserId,
                    UpdateStatus = (ApprenticeshipUpdateStatus)submission.UpdateStatus
                };

            await _mediator.SendAsync(command);
        }
    }
}
