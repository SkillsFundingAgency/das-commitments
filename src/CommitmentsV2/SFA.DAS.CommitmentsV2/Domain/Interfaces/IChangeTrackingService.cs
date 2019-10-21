using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IChangeTrackingService
    {
        void BeginTrackingSession(UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo);
        void TrackInsert(IMementoCreator trackedObject);
        void TrackUpdate(IMementoCreator trackedObject);
        void TrackDelete(IMementoCreator trackedObject);
        void CompleteTrackingSession();
    }
}
