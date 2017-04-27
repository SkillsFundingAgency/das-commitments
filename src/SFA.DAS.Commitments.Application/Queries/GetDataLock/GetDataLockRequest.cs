using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLock
{
    public sealed class GetDataLockRequest : IAsyncRequest<GetDataLockResponse>
    {
        public long DataLockEventId { get; set; }
    }
}
