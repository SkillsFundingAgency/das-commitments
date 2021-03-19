using MediatR;
using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit
{
    public class ValidateApprenticeshipForEditCommand : IRequest
    {
        public long? EmployerAccountId { get; set; }
        public long? ProviderId { get; set; }

        public long ApprenticeshipId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public decimal? Cost { get; set; }

        public string EmployerReference { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ULN { get; set; }

        public string TrainingCode { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}
