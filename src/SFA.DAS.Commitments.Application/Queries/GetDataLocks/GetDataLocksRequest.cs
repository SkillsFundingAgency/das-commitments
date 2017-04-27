using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLocks
{
    public sealed class GetDataLocksRequest : IAsyncRequest<GetDataLocksResponse>
    {
        public long ApprenticeshipId { get; set; }
    }
}
