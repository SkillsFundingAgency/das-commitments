using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortCommand : IRequest<AddCohortResponse>
    {
        public string UserId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }

        public string CourseCode { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OriginatorReference { get; set; }
        public Guid? ReservationId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ULN { get; set; }
    }
}
