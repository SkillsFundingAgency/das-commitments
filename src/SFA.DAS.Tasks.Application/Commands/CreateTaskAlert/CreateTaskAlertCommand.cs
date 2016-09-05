using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Commands.CreateTaskAlert
{
    public sealed class CreateTaskAlertCommand : IAsyncRequest
    {
        public long TaskId { get; set; }
        public string UserId { get; set; }
    }
}
