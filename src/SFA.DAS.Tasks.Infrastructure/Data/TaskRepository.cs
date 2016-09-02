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
                await c.ExecuteAsync("INSERT INTO [dbo].[Tasks](Assignee, TaskTemplateId, Name, CreatedOn) VALUES (@assignee, @taskTemplateId, @name, @createdOn);", task));
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

        public TaskAlert Create(TaskAlert taskAlert)
        {
            throw new NotImplementedException();
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
    }
}
