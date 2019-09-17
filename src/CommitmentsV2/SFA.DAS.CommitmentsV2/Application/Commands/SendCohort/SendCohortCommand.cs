using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.SendCohort
{
    public class SendCohortCommand : IRequest
    {
        public long CohortId { get; }
        public string Message { get; }
        public UserInfo UserInfo { get; }

        public SendCohortCommand(long cohortId, string message, UserInfo userInfo)
        {
            CohortId = cohortId;
            Message = message;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }
    }
}