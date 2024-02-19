using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipConfirmationStatus : Aggregate
    {
        public ApprenticeshipConfirmationStatus()
        {
        }

        public ApprenticeshipConfirmationStatus Copy()
        {
            return new ApprenticeshipConfirmationStatus
            {
                CommitmentsApprovedOn = this.CommitmentsApprovedOn,
                ApprenticeshipConfirmedOn = this.ApprenticeshipConfirmedOn,
                ConfirmationOverdueOn = this.ConfirmationOverdueOn,
            };
        }

        public ApprenticeshipConfirmationStatus(long apprenticeshipId, DateTime commitmentsApprovedOn, DateTime? confirmationOverdueOn, DateTime? apprenticeshipConfirmedOn)
        {
            ApprenticeshipId = apprenticeshipId;
            CommitmentsApprovedOn = commitmentsApprovedOn;
            ApprenticeshipConfirmedOn = apprenticeshipConfirmedOn;
            ConfirmationOverdueOn = confirmationOverdueOn;
        }

        public long ApprenticeshipId { get; set; }
        public ApprenticeshipBase Apprenticeship { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public DateTime? ConfirmationOverdueOn { get; set; }
        public DateTime? ApprenticeshipConfirmedOn { get; set; }
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