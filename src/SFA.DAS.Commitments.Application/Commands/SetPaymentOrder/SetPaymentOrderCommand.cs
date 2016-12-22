using System;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.SetPaymentOrder
{
    public sealed class SetPaymentOrderCommand : IAsyncRequest
    {
        public long AccountId { get; set; }
    }
}
