using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    /// <summary>
    ///     This represents a service that can publish the V2 commands via nservicebus.
    /// </summary>
    public interface INotificationsPublisher
    {
        Task ProviderAmendedCohort(Commitment commitment);
        Task ProviderApprovedCohort(Commitment commitment);
    }
}
