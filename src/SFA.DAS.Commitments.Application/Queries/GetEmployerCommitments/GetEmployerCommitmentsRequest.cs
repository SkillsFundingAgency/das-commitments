using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments
{
    public sealed class GetEmployerCommitmentsRequest : IAsyncRequest<GetEmployerCommitmentsResponse>
    {
        public long AccountId { get; set; }
    }
}
