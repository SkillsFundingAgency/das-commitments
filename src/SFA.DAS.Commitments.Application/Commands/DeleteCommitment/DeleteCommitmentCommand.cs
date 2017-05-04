﻿using System;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.DeleteCommitment
{
    public sealed class DeleteCommitmentCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
