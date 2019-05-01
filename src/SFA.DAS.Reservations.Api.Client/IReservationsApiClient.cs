using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.Reservations.Api.Client
{
    public interface IReservationsApiClient
    {
        Task<ValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken);
    }
}
