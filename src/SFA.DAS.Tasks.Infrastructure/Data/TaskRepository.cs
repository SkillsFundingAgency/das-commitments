using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using SFA.DAS.Tasks.Infrastructure.Configuration;
using Task = SFA.DAS.Tasks.Domain.Entities.Task;

namespace SFA.DAS.Tasks.Infrastructure.Data
{
    public class TaskRepository : ITaskRepository
    {
        public TaskRepository(TasksConfiguration configuration) {}

        public Task Create(Task task)
        {
            throw new NotImplementedException();
        }

        public Task<Task> GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Task>> GetByAssignee(string assignee)
        {
            throw new NotImplementedException();
        }

        public TaskAlert Create(TaskAlert taskAlert)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TaskAlert>> GetByUser(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
