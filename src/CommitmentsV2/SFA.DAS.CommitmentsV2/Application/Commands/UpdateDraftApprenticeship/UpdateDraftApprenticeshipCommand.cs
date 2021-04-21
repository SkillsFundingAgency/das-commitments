using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship
{
    public class UpdateDraftApprenticeshipCommand : IRequest<UpdateDraftApprenticeshipResponse>
    {
        public long CohortId { get; set; }
        public long ApprenticeshipId { get; set; }
        public string CourseCode { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reference { get; set; }
        public Guid? ReservationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Uln { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
