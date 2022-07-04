using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class OverlappingTrainingDateRequest : Aggregate, ITrackableEntity
    {
        public virtual long Id { get; private set; }
        public virtual long ApprenticeshipId { get; private set; }
        public virtual long PreviousApprenticeshipId { get; private set; }
        public virtual OverlappingTrainingDateRequestResolutionType? ResolutionType { get; private set; }
        public virtual OverlappingTrainingDateRequestStatus Status { get; private set; }
        public virtual OverlappingTrainingDateRequestEmployerAction? EmployerAction { get; private set; }
    }
}
