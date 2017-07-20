using MediatR;
using System;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime DateOfChange { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
