using System;
using System.Collections.Generic;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskTemplates
{
    public sealed class GetTaskTemplatesResponse : QueryResponse<IList<TaskTemplate>> {}
}
