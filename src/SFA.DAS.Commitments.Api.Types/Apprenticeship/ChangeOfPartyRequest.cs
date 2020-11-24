using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Commitments.Api.Types.Apprenticeship
{
    public class ChangeOfPartyRequest
    {
        public long Id { get; set; }
        public ChangeOfPartyRequestType ChangeOfPartyType { get; set; }
        public Party OriginatingParty { get; set; }
        public ChangeOfPartyRequestStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Price { get; set; }
        public long? CohortId { get; set; }
        public Party? WithParty { get; set; }
        public long? NewApprenticeshipId { get; set; }
        public long? ProviderId { get; set; }
    }
}
