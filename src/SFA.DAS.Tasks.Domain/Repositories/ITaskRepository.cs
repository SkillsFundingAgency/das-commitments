using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Tasks.Domain.Entities;
using Task = SFA.DAS.Tasks.Domain.Entities.Task;

namespace SFA.DAS.Tasks.Domain.Repositories
{
    public interface ITaskRepository
    {
        System.Threading.Tasks.Task Create(Task task);

        Task<Task> GetById(long id);

        Task<IList<Task>> GetByAssignee(string assignee);

        System.Threading.Tasks.Task SetCompleted(Task task);

        System.Threading.Tasks.Task Create(TaskAlert taskAlert);

        Task<IList<TaskAlert>> GetByUser(string userId);

        Task<IList<TaskTemplate>> GetAll();

        System.Threading.Tasks.Task Create(TaskTemplate taskTemplate);

        Task<TaskTemplate> GetTemplateById(long taskTemplateId);
    }
}
