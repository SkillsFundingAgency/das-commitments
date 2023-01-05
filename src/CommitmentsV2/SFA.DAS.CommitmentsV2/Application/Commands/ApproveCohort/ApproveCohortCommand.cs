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
        public Party? RequestingParty { get; }

        public ApproveCohortCommand(long cohortId, string message, UserInfo userInfo, Party? requestingParty)
        {
            CohortId = cohortId;
            Message = message;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
            RequestingParty = requestingParty;
        }
    }
}