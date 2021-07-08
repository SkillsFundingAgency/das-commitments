using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed
{
    public class ApprenticeshipConfirmedCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }
        public DateTime ConfirmedOn { get; set; }

        public ApprenticeshipConfirmedCommand(long apprenticeshipId, DateTime commitmentsApprovedOn, DateTime confirmedOn)
        {
            ApprenticeshipId = apprenticeshipId;
            CommitmentsApprovedOn = commitmentsApprovedOn;
            ConfirmedOn = confirmedOn;
        }
    }
}