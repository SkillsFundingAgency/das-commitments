using System;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations
{
    public class ReservationValidationRequest
    {
        public long AccountId { get; private set; }
        public Guid ReservationId { get; private set; }
        public DateTime? StartDate { get; private set; }
        public long ProviderId { get; private set; }
        public string AccountLegalEntityPublicHashedId { get; private set; }
        public string CourseCode { get; private set; }

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
