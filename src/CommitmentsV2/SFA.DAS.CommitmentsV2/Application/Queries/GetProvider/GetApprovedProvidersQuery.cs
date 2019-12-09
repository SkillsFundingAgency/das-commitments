using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider
{
    public class GetApprovedProvidersQuery : IRequest<GetApprovedProvidersQueryResult>
    {
        public long? AccountId { get; set; }

        public GetApprovedProvidersQuery(long? accountId)
        {
            AccountId = accountId;
        }
    }
}
