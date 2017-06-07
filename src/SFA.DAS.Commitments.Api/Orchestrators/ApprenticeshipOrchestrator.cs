using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Application.Queries.GetPriceEpisodes;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ApprenticeshipsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public ApprenticeshipsOrchestrator(IMediator mediator, ICommitmentsLogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<GetDataLockResponse> GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            _logger.Info($"Getting data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            var response = await _mediator.SendAsync(new GetDataLockRequest
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId
            });

            return response;
        }

        public async Task<GetDataLocksResponse> GetDataLocks(long apprenticeshipId)
        {
            _logger.Info($"Getting data locks for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            var response = await _mediator.SendAsync(new GetDataLocksRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }

        public async Task PatchDataLock(long apprenticeshipId, long dataLockEventId, DataLockTriageSubmission triageSubmission)
        {
            _logger.Info($"Patching data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            await _mediator.SendAsync(new UpdateDataLockTriageStatusCommand
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId,
                TriageStatus = triageSubmission.TriageStatus,
                UserId = triageSubmission.UserId
            });
        }

        public async Task<GetPriceEpisodesResponse> GetPriceEpisodes(long apprenticeshipId)
        {
            _logger.Info($"Getting price episodes for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            var response = await _mediator.SendAsync(new GetPriceEpisodesRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }
    }
}