using System;
using MediatR;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Application.Commands.CreateTask
{
    public sealed class CreateTaskCommand : IAsyncRequest
    {
        public Task Task { get; set; }
    }
}
