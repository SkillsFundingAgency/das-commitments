using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId
{
    public class GetLastSubmissionEventIdQuery : IRequest<long?>
    {
    }
}
