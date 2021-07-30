using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.TriageDataLocks
{
    public sealed class TriageDataLocksCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public UserInfo UserInfo { get; set; }
        

        public TriageDataLocksCommand(long apprenticeshipId, TriageStatus triageStatus, UserInfo userInfo)
        {
            ApprenticeshipId = apprenticeshipId;
            TriageStatus = triageStatus;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }
    }
}
