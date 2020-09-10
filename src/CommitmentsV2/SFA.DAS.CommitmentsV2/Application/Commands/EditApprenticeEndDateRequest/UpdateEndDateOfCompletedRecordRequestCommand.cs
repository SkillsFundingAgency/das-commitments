using MediatR;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest
{
    public class UpdateEndDateOfCompletedRecordRequestCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public DateTime? EndDate { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
