using System;
using System.Collections.Generic;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Application.Queries.GetTasks
{
    public sealed class GetTasksResponse : QueryResponse<IList<Task>> {}
}
