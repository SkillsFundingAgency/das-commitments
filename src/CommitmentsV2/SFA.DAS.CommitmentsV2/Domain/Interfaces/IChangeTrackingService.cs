using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IChangeTrackingService
    {
        void BeginTrackingSession(UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo);
        void TrackInsert(object trackedObject);
        void TrackUpdate(object trackedObject);
        void TrackDelete(object trackedObject);
        void CompleteTrackingSession();
    }
}
