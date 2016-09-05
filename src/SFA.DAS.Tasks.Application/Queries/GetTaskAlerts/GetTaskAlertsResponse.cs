using System;
using System.Collections.Generic;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskAlerts
{
    public sealed class GetTaskAlertsResponse : QueryResponse<IList<TaskAlert>> {}
}
