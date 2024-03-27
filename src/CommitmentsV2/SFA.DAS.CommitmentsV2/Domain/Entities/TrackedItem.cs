using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class TrackedItem
    {
        public Dictionary<string, object> InitialState { get; private set; }
        public ITrackableEntity TrackedEntity { get; private set; }
        public ChangeTrackingOperation Operation { get; private set; }

        public static TrackedItem CreateInsertTrackedItem(ITrackableEntity trackedEntity)
        {
            return new TrackedItem
            {
                TrackedEntity = trackedEntity,
                Operation = ChangeTrackingOperation.Insert
            };
        }

        public static TrackedItem CreateDeleteTrackedItem(ITrackableEntity trackedEntity, Dictionary<string, object> initialState)
        {
            return new TrackedItem
            {
                TrackedEntity = trackedEntity,
                InitialState = initialState,
                Operation = ChangeTrackingOperation.Delete
            };
        }

        public static TrackedItem CreateUpdateTrackedItem(ITrackableEntity trackedEntity, Dictionary<string, object> initialState)
        {
            return new TrackedItem
            {
                TrackedEntity = trackedEntity,
                InitialState = initialState,
                Operation = ChangeTrackingOperation.Update
            };
        }
    }
}
