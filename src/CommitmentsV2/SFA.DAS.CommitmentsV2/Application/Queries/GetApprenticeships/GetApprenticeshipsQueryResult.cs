using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsQueryResult
    {
        public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }
        public int TotalApprenticeshipsFound { get; set; }
        public int TotalApprenticeshipsWithAlertsFound { get; set; }
        public int TotalApprenticeships { get; set; }

        public class ApprenticeshipDetails
        {
            public long Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Uln { get; set; }
            public string EmployerName { get; set; }
            public string CourseName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public PaymentStatus PaymentStatus { get; set; }
            public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
            public IEnumerable<Alerts> Alerts { get; set; }
        }
    }
}
