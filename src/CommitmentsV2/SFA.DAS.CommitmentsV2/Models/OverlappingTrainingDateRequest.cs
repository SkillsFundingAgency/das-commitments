using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class OverlappingTrainingDateRequest : Aggregate, ITrackableEntity
    {
        public virtual long Id { get; private set; }
        public virtual long DraftApprenticeshipId { get; private set; }
        public virtual long PreviousApprenticeshipId { get; private set; }
        public virtual string RequestCreatedByProviderEmail { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
        public virtual OverlappingTrainingDateRequestStatus Status { get; set; }
        public virtual DateTime? NotifiedServiceDeskOn { get; set; }
        public byte[] RowVersion { get; private set; }
        public DateTime? ActionedOn { get; set; }
        public virtual DraftApprenticeship DraftApprenticeship { get; private set; }
        public virtual Apprenticeship PreviousApprenticeship { get; private set; }

        public OverlappingTrainingDateRequest()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public OverlappingTrainingDateRequest(DraftApprenticeship draftApprenticeship, long previousApprenticeshipId, Party originatingParty, UserInfo userInfo, DateTime createdDate)
        {
            StartTrackingSession(UserAction.CreateOverlappingTrainingDateRequest, originatingParty, draftApprenticeship.Cohort.AccountLegalEntityId, draftApprenticeship.Cohort.ProviderId, userInfo);
            PreviousApprenticeshipId = previousApprenticeshipId;
            Status = OverlappingTrainingDateRequestStatus.Pending;
            CreatedOn = createdDate;
            EmitOverlappingTrainingDateNotificationEvent(previousApprenticeshipId, draftApprenticeship.Uln);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        private void EmitOverlappingTrainingDateNotificationEvent(long apprenticeshipId, string uln)
        {
            Publish(() => new OverlappingTrainingDateCreatedEvent(apprenticeshipId, uln));
        }
    }
}