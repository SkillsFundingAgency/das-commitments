using System;
using SFA.DAS.CommitmentsV2.Models.Interfaces;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipConfirmationStatus : Aggregate, ITrackableEntity
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

        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public DateTime? ConfirmationOverdueOn { get; set; }
        public DateTime? ApprenticeshipConfirmedOn { get; set; }

        public ConfirmationStatus ConfirmationStatus => ApprenticeshipConfirmedOn == null
            ? ConfirmationStatus.Unconfirmed
            : ConfirmationStatus.Confirmed;

        public void SetStatusToUnconfirmedIfChangeIsLatest(DateTime newCommitmentsApprovedOn, DateTime newConfirmationOverdueOn)
        {
            if (CommitmentsApprovedOn < newCommitmentsApprovedOn)
            {
                CommitmentsApprovedOn = newCommitmentsApprovedOn;
                ConfirmationOverdueOn = newConfirmationOverdueOn;
                ApprenticeshipConfirmedOn = null;
            }
        }

        public void SetStatusToConfirmedIfChangeIsLatest(DateTime newCommitmentsApprovedOn, DateTime apprenticeshipConfirmedOn)
        {
            if (CommitmentsApprovedOn <= newCommitmentsApprovedOn)
            {
                CommitmentsApprovedOn = newCommitmentsApprovedOn;
                ApprenticeshipConfirmedOn = apprenticeshipConfirmedOn;
                ConfirmationOverdueOn = null;
            }
        }
    }

    public enum ConfirmationStatus : short
    {
        Unconfirmed = 0,
        Confirmed = 1
    }
}
