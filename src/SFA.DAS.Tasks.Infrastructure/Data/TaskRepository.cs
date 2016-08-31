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
        public TaskRepository(TaskConfiguration configuration) {}

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
            return System.Threading.Tasks.Task.Run(() => new[]
            {
                new Task {Assignee = assignee, Created = DateTime.UtcNow, Name = "Task 1", Id = 1},
                new Task {Assignee = assignee, Created = DateTime.UtcNow, Name = "Task 2", Id = 2},
                new Task {Assignee = assignee, Created = DateTime.UtcNow, Name = "Task 3", Id = 3}
            } as IList<Task>);
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
