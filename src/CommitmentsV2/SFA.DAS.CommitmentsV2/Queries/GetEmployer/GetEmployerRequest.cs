using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Queries.GetEmployer
{
    public class GetEmployerRequest : IRequest<GetEmployerResponse>
    {
        public long AccountLegalEntityId { get; set; }
    }
}
