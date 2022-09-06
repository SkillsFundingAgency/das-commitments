using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship
{
    public class AddDraftApprenticeshipCommand : IRequest<AddDraftApprenticeshipResult>
    {
        public long CohortId { get; set; }
        public string UserId { get; set; }
        public long ProviderId { get; set; }
        public string CourseCode { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
        public int? EmploymentPrice { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OriginatorReference { get; set; }
        public Guid? ReservationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Uln { get; set; }
        public UserInfo UserInfo { get; set; }
        public bool IgnoreStartDateOverlap { get; set; }
        public bool? IsOnFlexiPaymentPilot { get; set; }
    }
}