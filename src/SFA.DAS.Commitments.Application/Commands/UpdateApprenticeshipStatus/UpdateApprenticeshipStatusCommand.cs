using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using System;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommand : IAsyncRequest
    {
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime DateOfChange { get; set; }
        public string UserId { get; set; }
    }
}
