using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents
{
    public class GetSubmissionEventQueryHandler : IRequestHandler<GetSubmissionEventsQuery, PageOfResults<SubmissionEvent>>
    {
        public Task<PageOfResults<SubmissionEvent>> Handle(GetSubmissionEventsQuery request, CancellationToken cancellationToken)
        {
            PageOfResults<SubmissionEvent> pageOfResults = new PageOfResults<SubmissionEvent>();
            return Task.FromResult(pageOfResults);
        }
    }
}
