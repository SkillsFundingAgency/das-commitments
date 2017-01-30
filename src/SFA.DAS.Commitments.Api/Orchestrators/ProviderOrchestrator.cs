using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ProviderOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public ProviderOrchestrator(IMediator mediator, ICommitmentsLogger logger)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _mediator = mediator;
            _logger = logger;
        }

        public async Task<GetCommitmentsResponse> GetCommitments(long providerId)
        {
            _logger.Info($"Getting commitments for provider {providerId}", providerId: providerId);

            return await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });
        }

        public async Task<GetCommitmentResponse> GetCommitment(long providerId, long commitmentId)
        {
            _logger.Info($"Getting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            return await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId
            });
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long providerId)
        {
            _logger.Info($"Getting apprenticeships for provider {providerId}", providerId: providerId);

            return await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            _logger.Info($"Getting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            return await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });
        }

        public async Task<long> CreateApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            _logger.Info($"Creating apprenticeship for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            apprenticeship.CommitmentId = commitmentId;

            return await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            _logger.Info($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task CreateApprenticeships(long providerId, long commitmentId, IList<Apprenticeship> apprenticeships)
        {
            _logger.Info($"Bulk uploading {apprenticeships?.Count ?? 0} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller(providerId, CallerType.Provider),
                CommitmentId = commitmentId,
                Apprenticeships = apprenticeships
            });
        }

        public async Task PatchCommitment(long providerId, long commitmentId, LastAction latestAction)
        {
            _logger.Info($"Updating latest action to {latestAction} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                LatestAction = latestAction
            });
        }

        public async Task DeleteApprenticeship(long providerId, long apprenticeshipId)
        {
            _logger.Info($"Deleting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new DeleteApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });
        }

        public async Task DeleteCommitment(long providerId, long commitmentId)
        {
            _logger.Info($"Deleting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new DeleteCommitmentCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = providerId
                },
                CommitmentId = commitmentId
            });
        }
    }
}
