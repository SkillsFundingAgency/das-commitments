using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Application.Queries.GetPriceHistory;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage;
using SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage;
using SFA.DAS.Commitments.Application.Commands.TriageDataLock;
using SFA.DAS.Commitments.Application.Commands.TriageDataLocks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IDataLockMapper _dataLockMapper;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentsLogger _logger;

        public ApprenticeshipsOrchestrator(
            IMediator mediator, 
            IDataLockMapper dataLockMapper,
            IApprenticeshipMapper apprenticeshipMapper,
            ICommitmentsLogger logger)
        {
            _mediator = mediator;
            _dataLockMapper = dataLockMapper;
            _apprenticeshipMapper = apprenticeshipMapper;
            _logger = logger;
        }

        public async Task<Types.DataLock.DataLockStatus> GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            _logger.Trace($"Getting data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetDataLockRequest
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId
            });

            _logger.Info($"Retrieved data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId);

            return _dataLockMapper.Map(response.Data);
        }

        public async Task<IEnumerable<Types.DataLock.DataLockStatus>> GetDataLocks(long apprenticeshipId, Caller caller)
        {
            _logger.Trace($"Getting data locks for apprenticeship: {apprenticeshipId}", apprenticeshipId, caller: caller);

            var response = await _mediator.SendAsync(new GetDataLocksRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            _logger.Info($"Retrieved {response.Data.Count} data locks for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);

            return response.Data.Select(_dataLockMapper.Map);
        }
        
        public async Task<DataLockSummary> GetDataLockSummary(long apprenticeshipId, Caller caller)
        {
            _logger.Trace($"Getting data lock summary for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);

            var response = await _mediator.SendAsync(new GetDataLocksRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            var courseMismatch = response.Data
                .Where(DataLockExtensions.UnHandled)
                .Where(DataLockExtensions.WithCourseError).ToList();

            var withPriceOnly = response.Data
                .Where(DataLockExtensions.UnHandled)
                .Where(DataLockExtensions.IsPriceOnly).ToList();

            var summary = new DataLockSummary
            {
                DataLockWithCourseMismatch = courseMismatch.Select(_dataLockMapper.Map),
                DataLockWithOnlyPriceMismatch = withPriceOnly.Select(_dataLockMapper.Map)
            };

            _logger.Info($"Retrieved data lock summary for apprenticeship: {apprenticeshipId}. {summary.DataLockWithCourseMismatch.Count(): CourseMismatch}, {summary.DataLockWithOnlyPriceMismatch}: Price Mismatch ", apprenticeshipId: apprenticeshipId, caller: caller);

            return summary;
        }

        public async Task TriageDataLock(long apprenticeshipId, long dataLockEventId, DataLockTriageSubmission triageSubmission, Caller caller)
        {
            _logger.Trace($"Updating data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId} to triage status {triageSubmission.TriageStatus}", apprenticeshipId: apprenticeshipId, caller: caller);

            await _mediator.SendAsync(new TriageDataLockCommand
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId,
                TriageStatus = (TriageStatus)triageSubmission.TriageStatus,
                UserId = triageSubmission.UserId
            });

            _logger.Info($"Updated data lock: {dataLockEventId} for apprenticeship: {apprenticeshipId} to status: {triageSubmission.TriageStatus}", apprenticeshipId: apprenticeshipId, caller: caller);
        }

        public async Task TriageDataLocks(long apprenticeshipId, DataLockTriageSubmission triageSubmission, Caller caller)
        {
            _logger.Trace($"Updating all data locks to triage status: {triageSubmission.TriageStatus}, for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);

            await _mediator.SendAsync(new TriageDataLocksCommand
            {
                ApprenticeshipId = apprenticeshipId,
                TriageStatus = (TriageStatus)triageSubmission.TriageStatus,
                UserId = triageSubmission.UserId
            });

            _logger.Info($"Updated all data locks to triage status: {triageSubmission.TriageStatus}, for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);
        }

        public async Task ResolveDataLock(long apprenticeshipId, DataLocksTriageResolutionSubmission triageSubmission, Caller caller)
        {
            _logger.Trace($"Resolving datalock to: ({triageSubmission.DataLockUpdateType}), for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);

            if (triageSubmission.DataLockUpdateType == Types.DataLock.Types.DataLockUpdateType.ApproveChanges)
            {
                await _mediator.SendAsync(new ApproveDataLockTriageCommand
                {
                    ApprenticeshipId = apprenticeshipId,
                    UserId = triageSubmission.UserId,
                    Caller = caller
                });
            }
            else
            {
                await _mediator.SendAsync(new RejectDataLockTriageCommand
                {
                    ApprenticeshipId = apprenticeshipId,
                    UserId = triageSubmission.UserId
                });
            }

            _logger.Info($"Resolved datalock to: ({triageSubmission.DataLockUpdateType}), for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);
        }

        public async Task<IEnumerable<Types.Apprenticeship.PriceHistory>> GetPriceHistory(long apprenticeshipId, Caller caller)
        {
            _logger.Trace($"Getting price history for apprenticeship: {apprenticeshipId}", apprenticeshipId: apprenticeshipId, caller: caller);

            var response = await _mediator.SendAsync(new GetPriceHistoryRequest
            {
                Caller = caller,
                ApprenticeshipId = apprenticeshipId
            });

            _logger.Info($"Retrieved {response.Data.Count()} price history items for apprenticeship: {apprenticeshipId}"
                , apprenticeshipId: apprenticeshipId, caller: caller);

            return response.Data.Select(_apprenticeshipMapper.MapPriceHistory);
        }
    }
}