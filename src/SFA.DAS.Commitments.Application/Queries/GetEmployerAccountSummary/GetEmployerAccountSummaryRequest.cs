using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary
{
    public class GetEmployerAccountSummaryRequest : IAsyncRequest<GetEmployerAccountSummaryResponse>
    {
        public Caller Caller { get; set; }
    }
}
