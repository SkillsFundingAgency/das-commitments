using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort
{
    public class ApproveCohortCommand : IRequest
    {
        public long CohortId { get; }
        public string Message { get; }
        public UserInfo UserInfo { get; }
        public Party Party { get; }

        public ApproveCohortCommand(long cohortId, string message, UserInfo userInfo, Party party)
        {
            CohortId = cohortId;
            Message = message;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
            Party = party;
        }
    }
}