using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents
{
    public interface IApprenticeshipEventsPublisher
    {
        Task Publish(IApprenticeshipEventsList events);
    }
}
