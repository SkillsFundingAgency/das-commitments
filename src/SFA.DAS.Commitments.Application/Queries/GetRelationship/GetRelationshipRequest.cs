using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationship
{
    public class GetRelationshipRequest : IAsyncRequest<GetRelationshipResponse>
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public string LegalEntityId { get; set; }
    }
}
