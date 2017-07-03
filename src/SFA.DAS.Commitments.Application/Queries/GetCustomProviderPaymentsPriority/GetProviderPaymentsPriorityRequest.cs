using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityRequest : IAsyncRequest<GetProviderPaymentsPriorityResponse>
    {
        public Caller Caller { get; set; }

        public long EmployerAccountId { get; set; }

    }
}
