using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class ApprenticeshipDetailsResponse
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Uln { get; set; }
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string ApprenticeshipStatus { get; set; }
        public IEnumerable<string> Alerts { get; set; }
    }

}
