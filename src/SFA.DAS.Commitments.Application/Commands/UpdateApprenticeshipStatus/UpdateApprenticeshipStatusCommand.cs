using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using System;

using SFA.DAS.Commitments.Domain;

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
