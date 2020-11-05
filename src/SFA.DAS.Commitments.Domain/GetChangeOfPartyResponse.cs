using SFA.DAS.Commitments.Domain.Entities;
using System;

namespace SFA.DAS.Commitments.Domain
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
