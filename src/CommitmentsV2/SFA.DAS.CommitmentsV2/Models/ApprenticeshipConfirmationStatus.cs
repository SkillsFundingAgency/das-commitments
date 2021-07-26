using System;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipConfirmationStatus : Aggregate
    {
        public ApprenticeshipConfirmationStatus()
        {
        }

        public ApprenticeshipConfirmationStatus(long apprenticeshipId, DateTime commitmentsApprovedOn, DateTime? confirmationOverdueOn, DateTime? apprenticeshipConfirmedOn)
        {
            ApprenticeshipId = apprenticeshipId;
            CommitmentsApprovedOn = commitmentsApprovedOn;
            ConfirmationOverdueOn = confirmationOverdueOn;
            ApprenticeshipConfirmedOn = apprenticeshipConfirmedOn;
        }
        public long ApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public DateTime? ConfirmationOverdueOn { get; set; }
        public DateTime? ApprenticeshipConfirmedOn { get; set; }
        public Apprenticeship Apprenticeship { get; set; }
        public string ConfirmationStatusSort { get; set; }

        public ConfirmationStatus ConfirmationStatus => ApprenticeshipConfirmedOn == null
            ? ConfirmationStatus.Unconfirmed
            : ConfirmationStatus.Confirmed;

        public void SetStatusToUnconfirmedIfChangeIsLatest(DateTime newCommitmentsApprovedOn, DateTime newConfirmationOverdueOn)
        {
            if (CommitmentsApprovedOn < newCommitmentsApprovedOn.AddSeconds(-1))
            {
                CommitmentsApprovedOn = newCommitmentsApprovedOn;
                ConfirmationOverdueOn = newConfirmationOverdueOn;
                ApprenticeshipConfirmedOn = null;
            }
        }

        public void SetStatusToConfirmedIfChangeIsLatest(DateTime newCommitmentsApprovedOn, DateTime apprenticeshipConfirmedOn)
        {
            if (CommitmentsApprovedOn.AddSeconds(-1) <= newCommitmentsApprovedOn)
            {
                CommitmentsApprovedOn = newCommitmentsApprovedOn;
                ApprenticeshipConfirmedOn = apprenticeshipConfirmedOn;
            }
        }
    }
}
