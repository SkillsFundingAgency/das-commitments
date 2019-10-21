using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mementos;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class TrackedItem
    {
        public IMemento InitialState { get; }
        public IMementoCreator TrackedEntity { get; }
        public ChangeTrackingOperation Operation { get; }

        public TrackedItem(IMementoCreator trackedEntity,
            ChangeTrackingOperation operation)
        {
            TrackedEntity = trackedEntity;
            Operation = operation;

            if (Operation != ChangeTrackingOperation.Insert)
            {
                InitialState = trackedEntity.CreateMemento();
            }
        }
    }
}
