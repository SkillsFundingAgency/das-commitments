using SFA.DAS.CommitmentsV2.Models.Interfaces;

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
