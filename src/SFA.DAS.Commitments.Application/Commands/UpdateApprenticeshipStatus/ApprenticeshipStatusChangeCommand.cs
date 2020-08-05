using System;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public abstract class ApprenticeshipStatusChangeCommand : IAsyncRequest
    {
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public Caller Caller { get; set; }
        public DateTime DateOfChange { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}