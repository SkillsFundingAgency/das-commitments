using System;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Mementos
{
    public class DraftApprenticeshipMemento : IMemento
    {
        public string EntityName => nameof(DraftApprenticeship);
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CourseCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public Guid? ReservationId { get; set; }
    }
}
