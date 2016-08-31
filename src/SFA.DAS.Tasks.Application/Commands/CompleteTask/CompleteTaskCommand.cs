using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Commands.CompleteTask
{
    public sealed class CompleteTaskCommand : IAsyncRequest
    {
        public long TaskId { get; set; }
    }
}
