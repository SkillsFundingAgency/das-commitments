
using System;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ApprenticeshipSearchItemViewModel
    {
        public string HashedAccountId { get; set; }
        public string ApprenticeshipHashId { get; set; }
        public string ApprenticeName { get; set; }
        public long ProviderUkprn { get; set; }
        public string EmployerName { get; set; }
        public string TrainingDates { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Uln { get; set; }
        public string UlnText => string.IsNullOrEmpty(Uln) ? "Pending" : Uln;
    }
}