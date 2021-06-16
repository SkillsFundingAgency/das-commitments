using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class EntityStateChangedEvent
    {
        public Guid CorrelationId { get; set; }
        public UserAction StateChangeType { get; set; }
        public string EntityType { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public long EntityId { get; set; }
        public string InitialState { get; set; }
        public string UpdatedState { get; set; }
        public string UpdatingUserId { get; set; }
        public string UpdatingUserName { get; set; }
        public Party UpdatingParty { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public enum UserAction
    {
        ApproveCohort,
        SendCohort,
        AddDraftApprenticeship,
        UpdateDraftApprenticeship,
        CreateCohort,
        CreateCohortWithOtherParty,
        CreateCohortWithChangeOfParty,
        DeleteDraftApprenticeship,
		DeleteCohort,
        ApproveTransferRequest,
        RejectTransferRequest,
        CreateChangeOfPartyRequest,
        UpdateChangeOfPartyRequest,
        Complete,
        UpdateCompletionDate,
        SetCohortId,
        ApproveChangeOfPartyRequest,
        RejectChangeOfPartyRequest,
        WithdrawChangeOfPartyRequest,
        SetNewApprenticeshipId,
        EditEndDateOfCompletedApprentice,
        PauseApprenticeship,
        ResumeApprenticeship,
        StopApprenticeship,
        UpdateCustomProviderPaymentPriorities,
        UpdateApprenticeshipStopDate,
        Updated,
        EditedApprenticeship,
        UpdatePriceHistory,
        UpdateCourse,
        AcceptDataLockChange,
        RejectDataLockChange
    }
}
