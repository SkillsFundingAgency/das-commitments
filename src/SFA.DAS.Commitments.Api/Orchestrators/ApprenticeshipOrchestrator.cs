using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ApprenticeshipsOrchestrator
    {
        private readonly IMediator _mediator;

        public ApprenticeshipsOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<GetDataLockResponse> GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            var response = await _mediator.SendAsync(new GetDataLockRequest
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId
            });

            return response;
        }

        public async Task<GetDataLocksResponse> GetDataLocks(long apprenticeshipId)
        {
            var response = await _mediator.SendAsync(new GetDataLocksRequest
            {
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }

        public async Task PatchDataLock(long apprenticeshipId, long dataLockEventId, DataLockStatus datalock)
        {
            await _mediator.SendAsync(new UpdateDataLockCommand
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = dataLockEventId,
                DataLockStatus = datalock
            });
        }
    }
}