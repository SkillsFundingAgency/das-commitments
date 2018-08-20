using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements
{
    public sealed class GetCommitmentAgreementsRequest : IAsyncRequest<GetCommitmentAgreementsResponse>
    {
        public Caller Caller { get; set; }
    }
}
