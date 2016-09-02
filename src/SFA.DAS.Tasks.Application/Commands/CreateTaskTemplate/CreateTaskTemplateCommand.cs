using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Commands.CreateTaskTemplate
{
    public sealed class CreateTaskTemplateCommand : IAsyncRequest
    {
        public string Name { get; set; }
    }
}
