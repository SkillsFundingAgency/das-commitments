using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Commands.CreateTask
{
    public sealed class CreateTaskCommand : IAsyncRequest
    {
        public string Assignee { get; set; }
        public long TaskTemplateId { get; set; }
        public string Body { get; set; }
    }
}
