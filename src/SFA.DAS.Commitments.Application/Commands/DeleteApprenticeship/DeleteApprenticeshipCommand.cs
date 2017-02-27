using System;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship
{
    public sealed class DeleteApprenticeshipCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long ApprenticeshipId { get; set; }

        public string UserId { get; set; }
    }
}
