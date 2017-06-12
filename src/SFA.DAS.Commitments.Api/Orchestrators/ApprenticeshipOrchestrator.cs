using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Application.Queries.GetPriceHistory;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

using TriageStatus = SFA.DAS.Commitments.Api.Types.DataLock.Types.TriageStatus;

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

        public async Task PatchDataLocks(long apprenticeshipId, DataLocksTriageSubmission triageSubmission)
        {
            _logger.Info($"Updateing triange status: {triageSubmission.TriageStatus}, for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            await _mediator.SendAsync(new UpdateDataLocksTriageStatusCommand
            {
                ApprenticeshipId = apprenticeshipId,
                TriageStatus = (TriageStatus)triageSubmission.TriageStatus,
                UserId = triageSubmission.UserId
            });
        }

        public async Task PatchDataLocks(long apprenticeshipId, DataLocksTriageResolutionSubmission triageSubmission)
        {
            _logger.Info($"Patching ({triageSubmission.DataLockUpdateType}), for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            await _mediator.SendAsync(new UpdateDataLocksTriageResolutionCommand
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockUpdateType = triageSubmission.DataLockUpdateType,
                TriageStatus = triageSubmission.TriageStatus,
                UserId = triageSubmission.UserId
            });
        }

        public async Task<GetPriceHistoryResponse> GetPriceHistory(long apprenticeshipId)
        {
            _logger.Info($"Getting price history for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            var response = await _mediator.SendAsync(new GetPriceHistoryRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }
    }
}