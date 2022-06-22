using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ApprenticeshipViewModel
    {
        public string PaymentStatus { get; set; }
        public string AgreementStatus { get; set; }
        public string ConfirmationStatusDescription { get; set; }
        public IEnumerable<string> Alerts { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";

        public string Email { get; set; }
        public string Uln { get; set; }
        public string UlnText => string.IsNullOrEmpty(Uln) ? "Pending" : Uln;
        public DateTime? DateOfBirth { get; set; }
        public string CohortReference { get; set; }
        public string EmployerReference { get; set; } = string.Empty;

        public string LegalEntity { get; set; }

        public string TrainingProvider { get; set; }
        public long UKPRN { get; set; }
        public string Trainingcourse { get; set; }
        public string ApprenticeshipCode { get; set; }
        public string EndPointAssessor { get; set; }

        public DateTime? DasTrainingStartDate { get; set; }
        public DateTime? DasTrainingEndDate { get; set; }
        public DateTime? ILRTrainingStartDate { get; set; }
        public DateTime? ILRTrainingeEndDate { get; set; }

        public Decimal? TrainingCost { get; set; }

        public string Version { get; set; }
        public string Option { get; set; }
        public string PauseDate { get; set; }
        public string StopDate { get; set; }
        public string CompletionPaymentMonth { get; set; }
        public string PaymentStatusTagColour { get; set; }
        public bool? MadeRedundant { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
        public int? EmploymentPrice { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public ApprenticeshipUpdateViewModel ApprenticeshipUpdates { get; set; }

        public List<ApprenticeshipProviderHistoryViewModel> ApprenticeshipProviderHistory { get; set; } = new List<ApprenticeshipProviderHistoryViewModel>();
    }
}