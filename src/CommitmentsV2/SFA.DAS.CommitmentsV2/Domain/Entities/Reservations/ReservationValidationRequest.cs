using System;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations
{
    public class ReservationValidationRequest
    {
        public long AccountId { get; }
        public Guid ReservationId { get; }
        public DateTime? StartDate { get; }
        public long ProviderId { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public string CourseCode { get; }

        public ReservationValidationRequest(long providerId, long accountId, string accountLegalEntityPublicHashedId, Guid reservationId, DateTime? startDate, string courseCode)
        {
            ProviderId = providerId;
            AccountId = accountId;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            ReservationId = reservationId;
            StartDate = startDate;
            CourseCode = courseCode;
        }
    }
}
