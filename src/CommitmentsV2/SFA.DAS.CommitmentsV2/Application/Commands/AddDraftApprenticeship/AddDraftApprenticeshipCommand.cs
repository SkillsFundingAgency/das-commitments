using System;
using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship
{
    public class AddDraftApprenticeshipCommand : IRequest
    {
        public long CohortId { get; }
        public string UserId { get; }
        public long ProviderId { get; }
        public string CourseCode { get; }
        public int? Cost { get; }
        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }
        public string OriginatorReference { get; }
        public Guid? ReservationId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime? DateOfBirth { get; }
        public string Uln { get; }

        public AddDraftApprenticeshipCommand(long cohortId, string userId, long providerId, string courseCode, int? cost, DateTime? startDate, DateTime? endDate, string originatorReference, Guid? reservationId, string firstName, string lastName, DateTime? dateOfBirth, string uln)
        {
            CohortId = cohortId;
            UserId = userId;
            ProviderId = providerId;
            CourseCode = courseCode;
            Cost = cost;
            StartDate = startDate;
            EndDate = endDate;
            OriginatorReference = originatorReference;
            ReservationId = reservationId;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Uln = uln;
        }
    }
}