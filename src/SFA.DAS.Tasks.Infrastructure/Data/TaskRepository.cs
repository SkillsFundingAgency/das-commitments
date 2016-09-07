using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Tasks.Domain.Entities;
using SFA.DAS.Tasks.Domain.Repositories;
using SFA.DAS.Tasks.Infrastructure.Configuration;
using Task = System.Threading.Tasks.Task;

namespace SFA.DAS.Tasks.Infrastructure.Data
{
    public class TaskRepository : BaseRepository, ITaskRepository
    {
        public TaskRepository(TaskConfiguration configuration) : base(configuration) {}

        public async Task Create(Domain.Entities.Task task)
        {
            await WithConnection(async c =>
                await c.ExecuteAsync("INSERT INTO [dbo].[Tasks](Assignee, TaskTemplateId, Name, Body, CreatedOn) VALUES (@assignee, @taskTemplateId, @name, @body, @createdOn);", task));
        }

        public async Task<Domain.Entities.Task> GetById(long id)
        {
            return await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);

                var result = await c.QueryAsync<Domain.Entities.Task>("SELECT * FROM [dbo].[Tasks] WHERE Id = @id;", parameters);

                return result.FirstOrDefault();
            });
        }

        public async Task<IList<Domain.Entities.Task>> GetByAssignee(string assignee)
        {
            return await WithConnection<IList<Domain.Entities.Task>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@assignee", assignee);

                var results = await c.QueryAsync<Domain.Entities.Task>("SELECT * FROM [dbo].[Tasks] WHERE Assignee = @assignee;", parameters);

                return results.AsList();
            });
        }

        public async Task SetCompleted(Domain.Entities.Task task)
        {
            await WithConnection(async c =>
                await c.ExecuteAsync("UPDATE [dbo].[Tasks] SET CompletedOn = @completedOn, CompletedBy = @completedBy, TaskStatus = @taskStatus WHERE Id = @id;", task));
        }

        public async Task Create(TaskAlert taskAlert)
        {
            await WithConnection(async c =>
                await c.ExecuteAsync("INSERT INTO [dbo].[TaskAlerts](TaskId, UserId, CreatedOn) VALUES (@taskId, @userId, @createdOn);", taskAlert));
        }

        public async Task<IList<TaskAlert>> GetByUser(string userId)
        {
            return await WithConnection<IList<TaskAlert>>(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);

                var results = await c.QueryAsync<TaskAlert>("SELECT * FROM [dbo].[TaskAlerts] WHERE UserId = @userId;", parameters);

                return results.AsList();
            });
        }

        public async Task<IList<TaskTemplate>> GetAll()
        {
            return await WithConnection<IList<TaskTemplate>>(async c =>
            {
                var results = await c.QueryAsync<TaskTemplate>("SELECT * FROM [dbo].[TaskTemplates];");

                return results.AsList();
            });
        }

        public async Task Create(TaskTemplate taskTemplate)
        {
            await WithConnection(async c =>
                await c.ExecuteAsync("INSERT INTO [dbo].[TaskTemplates](Name) VALUES (@name);", taskTemplate));
        }

        public async Task<TaskTemplate> GetTemplateById(long taskTemplateId)
        {
            return await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", taskTemplateId);

                var result = await c.QueryAsync<TaskTemplate>("SELECT * FROM [dbo].[TaskTemplates] WHERE Id = @id;", parameters);

                return result.FirstOrDefault();
            });
        }
    }
}
