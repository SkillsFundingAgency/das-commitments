using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitments
{
    public sealed class GetCommitmentsRequest : IAsyncRequest<GetCommitmentsResponse>
    {
        public Caller Caller { get; set; }
    }
}
