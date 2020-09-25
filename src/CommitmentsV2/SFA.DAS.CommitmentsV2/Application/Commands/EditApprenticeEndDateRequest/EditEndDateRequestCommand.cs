using MediatR;
using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest
{
    public class EditEndDateRequestCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public DateTime? EndDate { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
