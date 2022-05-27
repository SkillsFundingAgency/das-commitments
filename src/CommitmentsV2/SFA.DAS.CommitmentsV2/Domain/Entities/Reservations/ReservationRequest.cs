using System;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations
{
    public class ReservationRequest
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? ProviderId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public bool IsLevyAccount { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public Guid? UserId { get; set; }
        public int RowNumber { get; set; }

        public static implicit operator DAS.Reservations.Api.Types.BulkReservation(ReservationRequest source)
        {
            return new DAS.Reservations.Api.Types.BulkReservation
            {
                Id = source.Id,
                StartDate = source.StartDate,
                CourseId = source.CourseId,
                ProviderId = source.ProviderId,
                AccountLegalEntityId = source.AccountLegalEntityId,
                TransferSenderAccountId = source.TransferSenderAccountId,
                UserId = source.UserId,
                RowNumber = source.RowNumber,
            };
        }
    }
}
