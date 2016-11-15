using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application
{
    public sealed class ApprenticeshipStateTransitionValidator : IValidateStateTransition<PaymentStatus>
    {
        public bool IsStateTransitionValid(PaymentStatus initial, PaymentStatus target)
        {
            if (initial == target)
                return true;

            if (target == PaymentStatus.Active)
            {
                return initial == PaymentStatus.PendingApproval || initial == PaymentStatus.Paused;
            }

            if (target == PaymentStatus.Paused)
            {
                return initial == PaymentStatus.Active;
            }

            return false;
        }
    }
}
