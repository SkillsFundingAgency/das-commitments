using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class TrackedItem
    {
        public Dictionary<string, object> InitialState { get; private set; }
        public object TrackedEntity { get; private set; }
        public ChangeTrackingOperation Operation { get; private set; }

        public static TrackedItem CreateInsertTrackedItem(object trackedEntity)
        {
            return new TrackedItem
            {
                TrackedEntity = trackedEntity,
                Operation = ChangeTrackingOperation.Insert
            };
        }

        public static TrackedItem CreateDeleteTrackedItem(object trackedEntity)
        {
            return new TrackedItem
            {
                TrackedEntity = trackedEntity,
                Operation = ChangeTrackingOperation.Delete
            };
        }

        public static TrackedItem CreateUpdateTrackedItem(object trackedEntity, Dictionary<string, object> initialState)
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
