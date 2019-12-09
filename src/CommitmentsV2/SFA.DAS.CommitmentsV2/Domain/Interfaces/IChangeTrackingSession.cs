using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IChangeTrackingSession
    {
        void TrackInsert(ITrackableEntity trackedObject);
        void TrackUpdate(ITrackableEntity trackedObject);
        void TrackDelete(ITrackableEntity trackedObject);
        void CompleteTrackingSession();
    }
}
