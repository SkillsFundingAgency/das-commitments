using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest
{
    public class GetChangeOfPartyRequestQuery : IRequest<GetChangeOfPartyRequestQueryResult>
    {
        public long ChangeOfPartyRequestId { get; }

        public GetChangeOfPartyRequestQuery(long changeOfPartyRequestId)
        {
            ChangeOfPartyRequestId = changeOfPartyRequestId;
        }
    }
}
