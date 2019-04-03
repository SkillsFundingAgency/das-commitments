using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    /// <summary>
    ///     This represents a service that can publish the V2 events via nservicebus.
    /// </summary>
    public interface IV2EventsPublisher
    {
        Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship);
    }
}
