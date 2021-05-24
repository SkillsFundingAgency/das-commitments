using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Api.Types
{
    public interface IReservationsApiClient
    {
        Task Ping(CancellationToken cancellationToken);
        Task<ReservationValidationResult> ValidateReservation(ReservationValidationMessage request, CancellationToken cancellationToken);
        Task<ReservationAllocationStatusResult> GetReservationAllocationStatus(ReservationAllocationStatusMessage request, CancellationToken cancellationToken);
        Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntity, BulkCreateReservationsRequest request, CancellationToken cancellationToken);
        Task<CreateChangeOfPartyReservationResult> CreateChangeOfPartyReservation(Guid reservationId, CreateChangeOfPartyReservationRequest request, CancellationToken cancellationToken);
        Task<bool> IsLevyAccount(long accountLegalEntityId, CancellationToken cancellationToken);
        Task<Guid> CreateReservationNonLevy(Reservation reservation, CancellationToken cancellationToken);
        Task<BulkValidationResults> BulkValidate(IEnumerable<Reservation> request, CancellationToken cancellationToken);
    }
}
