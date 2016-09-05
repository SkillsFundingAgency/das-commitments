using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Tasks.Domain.Repositories;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskTemplates
{
    public sealed class GetTaskTemplatesQueryHandler : IAsyncRequestHandler<GetTaskTemplatesRequest, GetTaskTemplatesResponse>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTaskTemplatesQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<GetTaskTemplatesResponse> Handle(GetTaskTemplatesRequest message)
        {
            var taskTemplates = await _taskRepository.GetAll();

            return new GetTaskTemplatesResponse {Data = taskTemplates};
        }
    }
}
