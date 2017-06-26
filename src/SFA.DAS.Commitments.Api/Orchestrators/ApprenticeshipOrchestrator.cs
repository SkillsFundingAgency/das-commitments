using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Application.Queries.GetPriceHistory;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;

using DataLocksTriageResolutionSubmission = SFA.DAS.Commitments.Api.Types.DataLock.DataLocksTriageResolutionSubmission;
using DataLockStatus = SFA.DAS.Commitments.Domain.Entities.DataLock.DataLockStatus;
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

        public async Task<IList<Api.Types.DataLock.DataLockStatus>> GetDataLocks(long apprenticeshipId)
        {
            _logger.Info($"Getting data locks for apprenticeship: {apprenticeshipId}", apprenticeshipId);

            var response = await _mediator.SendAsync(new GetDataLocksRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            return MapToApiType(response.Data);
        }

        public async Task<DataLockSummary> GetDataLockSummary(long apprenticeshipId)
        {
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
            return new DataLockSummary
                       {
                           DataLockWithCourseMismatch = MapToApiType(courseMismatch),
                           DataLockWithOnlyPriceMismatch = MapToApiType(withPriceOnly)
                       };
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

        private IList<Api.Types.DataLock.DataLockStatus> MapToApiType(IList<DataLockStatus> sourceList)
        {
            return sourceList.Select(source => new Api.Types.DataLock.DataLockStatus
            {
                ApprenticeshipId = source.ApprenticeshipId,
                DataLockEventDatetime = source.DataLockEventDatetime,
                DataLockEventId = source.DataLockEventId,
                ErrorCode = (Api.Types.DataLock.Types.DataLockErrorCode)source.ErrorCode,
                IlrActualStartDate = source.IlrActualStartDate,
                IlrEffectiveFromDate = source.IlrEffectiveFromDate,
                IlrTotalCost = source.IlrTotalCost,
                IlrTrainingCourseCode = source.IlrTrainingCourseCode,
                IlrTrainingType = (TrainingType)source.IlrTrainingType,
                PriceEpisodeIdentifier = source.PriceEpisodeIdentifier,
                Status = (Api.Types.DataLock.Types.Status)source.Status,
                TriageStatus = (Api.Types.DataLock.Types.TriageStatus)source.TriageStatus,
                ApprenticeshipUpdateId = source.ApprenticeshipUpdateId,
                IsResolved = source.IsResolved
            }).ToList();
        }
    }
}