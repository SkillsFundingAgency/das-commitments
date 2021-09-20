using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class OverlappingEmail
    {
        public long RowId { get; set; }
        public long? Id { get; set; }
        public long? CohortId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsApproved { get; set; }
        public string Email { get; set; }
        public OverlapStatus OverlapStatus { get; set; }
    }

    public enum OverlapStatus : short
    {
        None = 0,
        OverlappingStartDate = 1,
        OverlappingEndDate = 2,
        DateEmbrace = 3,
        DateWithin = 4
    }
}
