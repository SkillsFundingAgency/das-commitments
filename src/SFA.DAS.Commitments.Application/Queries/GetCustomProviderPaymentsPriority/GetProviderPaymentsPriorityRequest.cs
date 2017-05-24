using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityRequest : IAsyncRequest<GetProviderPaymentsPriorityResponse>
    {
        public long EmployerAccountId { get; set; }
    }
}
