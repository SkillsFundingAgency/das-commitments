using System.Collections.Generic;
using MediatR;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority
{
    public sealed class UpdateProviderPaymentsPriorityCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long EmployerAccountId { get; set; }
        public List<ProviderPaymentPriorityUpdateItem> ProviderPriorities { get; set; }

    }
}
