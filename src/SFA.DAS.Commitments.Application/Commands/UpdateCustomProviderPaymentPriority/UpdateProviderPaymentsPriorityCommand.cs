using System.Collections.Generic;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority
{
    public sealed class UpdateProviderPaymentsPriorityCommand : IAsyncRequest
    {
        public long EmployerAccountId { get; set; }
        public List<long> ProviderPriorities { get; set; }
    }
}
