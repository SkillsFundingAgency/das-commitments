using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ApprenticeshipOrchestrator
    {
        private IMediator _mediator;

        public ApprenticeshipOrchestrator(IMediator mediator)
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

        public async Task PatchDataLock(long apprenticeshipId, DataLockStatus datalock)
        {
            throw new System.NotImplementedException();
        }
    }
}