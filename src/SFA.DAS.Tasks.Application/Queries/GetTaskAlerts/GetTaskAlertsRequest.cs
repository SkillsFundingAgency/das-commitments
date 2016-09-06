using System;
using MediatR;

namespace SFA.DAS.Tasks.Application.Queries.GetTaskAlerts
{
    public sealed class GetTaskAlertsRequest : IAsyncRequest<GetTaskAlertsResponse>
    {
        public string UserId { get; set; }
    }
}
