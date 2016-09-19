using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application
{
    public sealed class ApprenticeshipStateTransitionValidator : IValidateStateTransition<ApprenticeshipStatus>
    {
        public bool IsStateTransitionValid(ApprenticeshipStatus initial, ApprenticeshipStatus target)
        {
            if (initial == target)
                return true;

            if (target == ApprenticeshipStatus.Approved)
            {
                return initial == ApprenticeshipStatus.ReadyForApproval;
            }

            return false;
        }
    }
}
