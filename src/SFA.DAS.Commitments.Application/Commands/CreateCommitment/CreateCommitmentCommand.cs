﻿using MediatR;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    // Note: Have currently broken the CQRS pattern here as need to return the Id.
    public sealed class CreateCommitmentCommand : IAsyncRequest<long>
    {
        public Caller Caller { get; set; }

        public Commitment Commitment { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }
        public LastAction LastAction { get; set; }
    }
}
