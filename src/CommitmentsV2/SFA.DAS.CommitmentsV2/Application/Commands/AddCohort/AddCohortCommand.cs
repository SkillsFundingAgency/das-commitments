using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortCommand : IRequest<AddCohortResult>
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
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
        public UserInfo UserInfo { get; }

        public AddCohortCommand(long accountId, long accountLegalEntityId, long providerId, string courseCode, int? cost, DateTime? startDate, DateTime? endDate, string originatorReference, Guid? reservationId, string firstName, string lastName, DateTime? dateOfBirth, string uln, UserInfo userInfo)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
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
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }
    }
}