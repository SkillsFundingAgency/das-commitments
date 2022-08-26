using System;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class OverlappingTrainingDateRequest : Aggregate, ITrackableEntity
    {
        public virtual long Id { get; private set; }
        public virtual long DraftApprenticeshipId { get; private set; }
        public virtual long PreviousApprenticeshipId { get; private set; }
        public virtual OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
        public virtual OverlappingTrainingDateRequestStatus Status { get; set; }
        public byte[] RowVersion { get; private set; }
        public DateTime? ActionedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public virtual DraftApprenticeship DraftApprenticeship { get; private set; }
        public virtual Apprenticeship PreviousApprenticeship { get; private set; }

        public OverlappingTrainingDateRequest()
        { }

        public OverlappingTrainingDateRequest(DraftApprenticeship draftApprenticeship, long previousApprenticeshipId, Party originatingParty, UserInfo userInfo)
        {
            StartTrackingSession(UserAction.CreateOverlappingTrainingDateRequest, originatingParty, draftApprenticeship.Cohort.AccountLegalEntityId, draftApprenticeship.Cohort.ProviderId, userInfo);
            PreviousApprenticeshipId = previousApprenticeshipId;
            Status = OverlappingTrainingDateRequestStatus.Pending;

            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }
    }
}