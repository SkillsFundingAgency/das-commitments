using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced
{
    public class ApprenticeshipConfirmationCommencedCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public DateTime ConfirmationOverdueOn { get; set; }

        public ApprenticeshipConfirmationCommencedCommand(long apprenticeshipId, DateTime commitmentsApprovedOn, DateTime confirmationOverdueOn)
        {
            ApprenticeshipId = apprenticeshipId;
            CommitmentsApprovedOn = commitmentsApprovedOn;
            ConfirmationOverdueOn = confirmationOverdueOn;
        }
    }
}