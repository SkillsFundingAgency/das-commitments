using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskTemplates
{
    public sealed class GetTaskTemplatesRequest : IAsyncRequest<GetTaskTemplatesResponse> {}
}
