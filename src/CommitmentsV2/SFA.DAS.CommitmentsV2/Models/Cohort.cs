using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class Cohort : Aggregate, ITrackableEntity
{
    public Cohort()
    {
        Apprenticeships = new HashSet<ApprenticeshipBase>();
        Messages = new HashSet<Message>();
        TransferRequests = new HashSet<TransferRequest>();
        LastUpdatedOn = DateTime.UtcNow;
    }

    private Cohort(long providerId,
        long accountId,
        long accountLegalEntityId,
        long? transferSenderId,
        int? pledgeApplicationId,
        Party originatingParty, UserInfo userInfo) : this()
    {
        CheckIsEmployerOrProvider(originatingParty);

        EmployerAccountId = accountId;
        AccountLegalEntityId = accountLegalEntityId;
        ProviderId = providerId;
        TransferSenderId = transferSenderId;
        PledgeApplicationId = pledgeApplicationId;
        IsDraft = true;

        // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
        Reference = "";
        Originator = originatingParty.ToOriginator();
        UpdatedBy(originatingParty, userInfo);
        LastUpdatedOn = DateTime.UtcNow;
        CommitmentStatus = CommitmentStatus.New;
        CreatedOn = DateTime.UtcNow;
        LastAction = LastAction.None;
    }

    /// <summary>
    /// Creates an empty cohort without draft apprenticeship
    /// </summary>
    internal Cohort(long providerId,
        long accountId,
        long accountLegalEntityId,
        Party originatingParty,
        UserInfo userInfo) : this(providerId, accountId, accountLegalEntityId, null, null, originatingParty, userInfo)
    {
        WithParty = originatingParty;
        EditStatus = originatingParty.ToEditStatus();
        IsDraft = true;

        StartTrackingSession(UserAction.CreateCohort, originatingParty, accountId, providerId, userInfo);
        ChangeTrackingSession.TrackInsert(this);
        ChangeTrackingSession.CompleteTrackingSession();
    }

    /// <summary>
    /// Creates a cohort with a draft apprenticeship
    /// </summary>
    internal Cohort(long providerId,
        long accountId,
        long accountLegalEntityId,
        long? transferSenderId,
        int? pledgeApplicationId,
        DraftApprenticeshipDetails draftApprenticeshipDetails,
        Party originatingParty,
        UserInfo userInfo,
        int maxAgeAtApprenticeshipStart,
        bool ignoreStartDateOverlap = false) : this(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId, originatingParty, userInfo)
    {
        CheckDraftApprenticeshipDetails(draftApprenticeshipDetails);
        ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, false, maxAgeAtApprenticeshipStart);
        WithParty = originatingParty;
        EditStatus = originatingParty.ToEditStatus();
        IsDraft = true;

        var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, originatingParty);
        Apprenticeships.Add(draftApprenticeship);

        Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));

        StartTrackingSession(UserAction.CreateCohort, originatingParty, accountId, providerId, userInfo);
        ChangeTrackingSession.TrackInsert(this);
        ChangeTrackingSession.TrackInsert(draftApprenticeship);
        ChangeTrackingSession.CompleteTrackingSession();
    }

    /// <summary>
    /// Creates an empty cohort with other party
    /// </summary>
    internal Cohort(long providerId,
        long accountId,
        long accountLegalEntityId,
        long? transferSenderId,
        int? pledgeApplicationId,
        Party originatingParty,
        string message,
        UserInfo userInfo) : this(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId, originatingParty, userInfo)
    {
        CheckIsEmployer(originatingParty);
        IsDraft = false;

        WithParty = originatingParty.GetOtherParty();
        EditStatus = originatingParty.GetOtherParty().ToEditStatus();
        LastAction = LastAction.Amend;
        if (message != null)
        {
            AddMessage(message, originatingParty, userInfo);
        }

        Publish(() => new CohortAssignedToProviderEvent(Id, DateTime.UtcNow));

        StartTrackingSession(UserAction.CreateCohortWithOtherParty, originatingParty, accountId, providerId, userInfo);
        ChangeTrackingSession.TrackInsert(this);
        ChangeTrackingSession.CompleteTrackingSession();
    }

    /// <summary>
    /// Creates a Cohort from a Change of Party Request
    /// </summary>
    internal Cohort(long providerId,
        long accountId,
        long accountLegalEntityId,
        Apprenticeship apprenticeship,
        Guid? reservationId,
        ChangeOfPartyRequest changeOfPartyRequest,
        UserInfo userInfo, bool hasOverlappingTrainingDates)
        : this(providerId,
            accountId,
            accountLegalEntityId,
            null,
            null,
            changeOfPartyRequest.OriginatingParty,
            userInfo)
    {

        ChangeOfPartyRequestId = changeOfPartyRequest.Id;

        Approvals = changeOfPartyRequest.IsPreApproved(hasOverlappingTrainingDates);

        WithParty = hasOverlappingTrainingDates ? Party.Provider : changeOfPartyRequest.OriginatingParty.GetOtherParty();
        IsDraft = hasOverlappingTrainingDates;

        if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
        {
            TransferSenderId = apprenticeship.Cohort.TransferSenderId;
            PledgeApplicationId = apprenticeship.Cohort.PledgeApplicationId;
        }

        var draftApprenticeship = apprenticeship.CreateCopyForChangeOfParty(changeOfPartyRequest, reservationId);
        Apprenticeships.Add(draftApprenticeship);

        //Retained for backwards-compatibility:
        EditStatus = WithParty.ToEditStatus();
        LastAction = LastAction.Amend;
        CommitmentStatus = CommitmentStatus.Active;

        Publish(() => new CohortWithChangeOfPartyCreatedEvent(Id, changeOfPartyRequest.Id, changeOfPartyRequest.OriginatingParty, DateTime.UtcNow, userInfo));

        if (!hasOverlappingTrainingDates)
        {
            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
            {
                Publish(() => new CohortAssignedToEmployerEvent(Id, DateTime.UtcNow, Party.Provider));
            }
            else
            {
                Publish(() => new CohortAssignedToProviderEvent(Id, DateTime.UtcNow));
            }
        }

        Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));

        StartTrackingSession(UserAction.CreateCohortWithChangeOfParty, changeOfPartyRequest.OriginatingParty, accountId, providerId, userInfo);
        ChangeTrackingSession.TrackInsert(this);
        ChangeTrackingSession.TrackInsert(draftApprenticeship);
        ChangeTrackingSession.CompleteTrackingSession();
    }

    public virtual long Id { get; set; }
    public string Reference { get; set; }
    public long EmployerAccountId { get; set; }
    public long AccountLegalEntityId { get; set; }
    public long ProviderId { get; set; }
    public CommitmentStatus CommitmentStatus { get; set; }
    public EditStatus EditStatus { get; set; }
    public DateTime? CreatedOn { get; set; }
    public LastAction LastAction { get; set; }
    public string LastUpdatedByEmployerName { get; set; }
    public string LastUpdatedByEmployerEmail { get; set; }
    public string LastUpdatedByProviderName { get; set; }
    public string LastUpdatedByProviderEmail { get; set; }
    public long? TransferSenderId { get; set; }
    public int? PledgeApplicationId { get; set; }
    public TransferApprovalStatus? TransferApprovalStatus { get; set; }
    public DateTime? TransferApprovalActionedOn { get; set; }
    public Originator Originator { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDraft { get; set; }

    public byte[] RowVersion { get; set; }
    public DateTime LastUpdatedOn { get; set; }

    public virtual ICollection<ApprenticeshipBase> Apprenticeships { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<TransferRequest> TransferRequests { get; set; }
    public virtual Provider Provider { get; set; }
    public virtual AccountLegalEntity AccountLegalEntity { get; set; }

    public virtual Account TransferSender { get; set; }
    public virtual ChangeOfPartyRequest ChangeOfPartyRequest { get; set; }
    public virtual ApprenticeshipEmployerType? ApprenticeshipEmployerTypeOnApproval { get; set; }

    public IEnumerable<DraftApprenticeship> DraftApprenticeships => Apprenticeships.OfType<DraftApprenticeship>();

    public int DraftApprenticeshipCount => DraftApprenticeships.Count();

    public string LastMessage => Messages.OrderByDescending(x => x.Id).FirstOrDefault()?.Text;

    public virtual Party WithParty { get; set; }
    public virtual Party Approvals { get; set; }
    public DateTime? EmployerAndProviderApprovedOn { get; set; }
    public long? ChangeOfPartyRequestId { get; set; }
    public bool IsLinkedToChangeOfPartyRequest => ChangeOfPartyRequestId.HasValue;

    public virtual bool IsApprovedByAllParties => WithParty == Party.None; //todo: use new Approvals flag

    public DraftApprenticeship AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party creator, UserInfo userInfo, int maxAgeAtApprenticeshipStart)
    {
        CheckIsWithParty(creator);
        ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, false, maxAgeAtApprenticeshipStart);

        StartTrackingSession(UserAction.AddDraftApprenticeship, creator, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, creator);
        Apprenticeships.Add(draftApprenticeship);
        Approvals = Party.None;
        UpdatedBy(creator, userInfo);
        LastUpdatedOn = DateTime.UtcNow;

        ChangeTrackingSession.TrackInsert(draftApprenticeship);
        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));
        return draftApprenticeship;
    }

    public virtual void Approve(Party modifyingParty, string message, UserInfo userInfo, DateTime now, bool apprenticeEmailRequired = false)
    {
        CheckIsEmployerOrProviderOrTransferSender(modifyingParty);
        CheckIsWithParty(modifyingParty);
        CheckIsCompleteForParty(modifyingParty, apprenticeEmailRequired);

        StartTrackingSession(UserAction.ApproveCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        switch (modifyingParty)
        {
            case Party.Employer:
            case Party.Provider:
            {
                var otherParty = modifyingParty.GetOtherParty();
                var isApprovedByOtherParty = Approvals.HasFlag(otherParty);

                IsDraft = false;
                EditStatus = isApprovedByOtherParty ? EditStatus.Both : otherParty.ToEditStatus();
                WithParty = GetWithParty(otherParty, isApprovedByOtherParty);
                if (isApprovedByOtherParty) EmployerAndProviderApprovedOn = DateTime.UtcNow;
                LastAction = LastAction.Approve;
                CommitmentStatus = CommitmentStatus.Active;
                TransferApprovalStatus = GetTransferApprovalStatus(isApprovedByOtherParty);
                Approvals |= modifyingParty;
                AddMessage(message, modifyingParty, userInfo);
                UpdatedBy(modifyingParty, userInfo);
                LastUpdatedOn = DateTime.UtcNow;

                switch (WithParty)
                {
                    case Party.Employer:
                        Publish(() => new CohortAssignedToEmployerEvent(Id, now, modifyingParty));
                        break;
                    case Party.Provider:
                        Publish(() => new CohortAssignedToProviderEvent(Id, now));
                        break;
                    case Party.TransferSender:
                        Publish(() => new CohortTransferApprovalRequestedEvent(Id, now, modifyingParty));
                        break;
                }

                break;
            }
            case Party.TransferSender:
                TransferApprovalStatus = Types.TransferApprovalStatus.Approved;
                TransferApprovalActionedOn = now;
                Approvals |= modifyingParty;
                WithParty = Party.None;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(modifyingParty));
        }

        if (Approvals.HasFlag(Party.Provider) && modifyingParty == Party.Employer)
        {
            Publish(() => new CohortApprovedByEmployerEvent(Id, now));
        }

        if (IsApprovedByAllParties)
        {
            Publish(() => new CohortFullyApprovedEvent(Id, EmployerAccountId, ProviderId, now, modifyingParty, ChangeOfPartyRequestId, userInfo));

            if (ChangeOfPartyRequestId.HasValue)
            {
                Publish(() => new CohortWithChangeOfPartyFullyApprovedEvent(Id, ChangeOfPartyRequestId.Value, now, modifyingParty, userInfo));
            }
        }
        else if (ChangeOfPartyRequestId.HasValue)
        {
            Publish(() => new CohortWithChangeOfPartyUpdatedEvent(Id, userInfo));
        }

        ChangeTrackingSession.CompleteTrackingSession();
    }

    private Party GetWithParty(Party otherParty, bool isApprovedByOtherParty)
    {
        if (isApprovedByOtherParty && TransferSenderId.HasValue && ChangeOfPartyRequestId.HasValue)
        {
            return Party.None;
        }

        return isApprovedByOtherParty
            ? TransferSenderId.HasValue ? Party.TransferSender : Party.None
            : otherParty;
    }

    private TransferApprovalStatus? GetTransferApprovalStatus(bool isApprovedByOtherParty)
    {
        if (isApprovedByOtherParty && TransferSenderId.HasValue && ChangeOfPartyRequestId.HasValue)
        {
            return Types.TransferApprovalStatus.Approved;
        }
        return null;
    }

    public virtual void SendToOtherParty(Party modifyingParty, string message, UserInfo userInfo, DateTime now)
    {
        CheckIsEmployerOrProvider(modifyingParty);
        CheckIsWithParty(modifyingParty);

        StartTrackingSession(UserAction.SendCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        IsDraft = false;
        EditStatus = modifyingParty.GetOtherParty().ToEditStatus();
        WithParty = modifyingParty.GetOtherParty();
        LastAction = LastAction.Amend;
        CommitmentStatus = CommitmentStatus.Active;
        TransferApprovalStatus = null;
        AddMessage(message, modifyingParty, userInfo);
        UpdatedBy(modifyingParty, userInfo);
        LastUpdatedOn = DateTime.UtcNow;

        switch (WithParty)
        {
            case Party.Employer:
                Publish(() => new CohortAssignedToEmployerEvent(Id, now, modifyingParty));
                break;
            case Party.Provider:
                Publish(() => new CohortAssignedToProviderEvent(Id, now));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(EditStatus));
        }

        if (Approvals.HasFlag(Party.Provider))
        {
            Publish(() => new ApprovedCohortReturnedToProviderEvent(Id, now));
        }

        if (ChangeOfPartyRequestId.HasValue)
        {
            Publish(() => new CohortWithChangeOfPartyUpdatedEvent(Id, userInfo));
        }

        Approvals = Party.None;
        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void UpdateDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party modifyingParty, UserInfo userInfo, int maxAgeAtApprenticeshipStart)
    {
        CheckIsWithParty(modifyingParty);

        var existingDraftApprenticeship = GetDraftApprenticeship(draftApprenticeshipDetails.Id);

        ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, ChangeOfPartyRequestId.HasValue, maxAgeAtApprenticeshipStart);

        if (ChangeOfPartyRequestId.HasValue)
        {
            existingDraftApprenticeship.ValidateUpdateForChangeOfParty(draftApprenticeshipDetails);
        }

        StartTrackingSession(UserAction.UpdateDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);
        ChangeTrackingSession.TrackUpdate(existingDraftApprenticeship);

        if (existingDraftApprenticeship.IsOtherPartyApprovalRequiredForUpdate(draftApprenticeshipDetails, modifyingParty))
        {
            Approvals = Party.None;
        }

        if (existingDraftApprenticeship.HasEmployerChangedCostWhereProviderHasSetTotalAndEPAPrice(
                draftApprenticeshipDetails, modifyingParty))
        {
            draftApprenticeshipDetails.TrainingPrice = null;
            draftApprenticeshipDetails.EndPointAssessmentPrice = null;
            draftApprenticeshipDetails.EmployerHasEditedCost = true;
        }

        if (draftApprenticeshipDetails.TrainingPrice != null && draftApprenticeshipDetails.EndPointAssessmentPrice != null)
            draftApprenticeshipDetails.EmployerHasEditedCost = false;

        existingDraftApprenticeship.Merge(draftApprenticeshipDetails, modifyingParty);

        UpdatedBy(modifyingParty, userInfo);
        LastUpdatedOn = DateTime.UtcNow;
        Publish(() => new DraftApprenticeshipUpdatedEvent(existingDraftApprenticeship.Id, Id, existingDraftApprenticeship.Uln, existingDraftApprenticeship.ReservationId, DateTime.UtcNow));
        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void AddTransferRequest(string jsonSummary, decimal cost, decimal fundingCap, Party lastApprovedByParty, bool autoApproval)
    {
        CheckThereIsNoPendingTransferRequest();
        var transferRequest = new TransferRequest(jsonSummary, cost, fundingCap, autoApproval);
        TransferRequests.Add(transferRequest);
        TransferApprovalStatus = Types.TransferApprovalStatus.Pending;
        Publish(() => new TransferRequestCreatedEvent(transferRequest.Id, Id, DateTime.UtcNow, lastApprovedByParty));
        if (autoApproval)
        {
            Publish(() => new TransferRequestWithAutoApprovalCreatedEvent(transferRequest.Id, PledgeApplicationId.Value, DateTime.UtcNow));
        }
    }

    public void Delete(Party modifyingParty, UserInfo userInfo)
    {
        CheckIsWithParty(modifyingParty);

        StartTrackingSession(UserAction.DeleteCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);
        LastUpdatedOn = DateTime.UtcNow;

        MarkAsDeletedAndEmitCohortDeletedEvent(modifyingParty, userInfo);

        foreach (var draftApprenticeship in DraftApprenticeships.ToArray())
        {
            RemoveDraftApprenticeship(draftApprenticeship);
        }

        ChangeTrackingSession.CompleteTrackingSession();
    }
    public void DeleteDraftApprenticeship(long draftApprenticeshipId, Party modifyingParty, UserInfo userInfo)
    {
        CheckIsWithParty(modifyingParty);

        var draftApprenticeship = DraftApprenticeships.Single(x => x.Id == draftApprenticeshipId);

        StartTrackingSession(UserAction.DeleteDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        RemoveDraftApprenticeship(draftApprenticeship);

        LastUpdatedOn = DateTime.UtcNow;
        Approvals = Party.None;
        ResetTransferSenderRejection();

        if (!DraftApprenticeships.Any() && !(modifyingParty == Party.Provider))
        {
            MarkAsDeletedAndEmitCohortDeletedEvent(modifyingParty, userInfo);
        }

        ChangeTrackingSession.CompleteTrackingSession();
    }

    private void RemoveDraftApprenticeship(DraftApprenticeship draftApprenticeship)
    {
        ChangeTrackingSession.TrackDelete(draftApprenticeship);
        LastUpdatedOn = DateTime.UtcNow;
        Apprenticeships.Remove(draftApprenticeship);
        Publish(() => new DraftApprenticeshipDeletedEvent
        {
            DraftApprenticeshipId = draftApprenticeship.Id,
            CohortId = draftApprenticeship.CommitmentId,
            Uln = draftApprenticeship.Uln,
            ReservationId = draftApprenticeship.ReservationId,
            DeletedOn = DateTime.UtcNow
        });
    }

    private void MarkAsDeletedAndEmitCohortDeletedEvent(Party deletedBy, UserInfo userInfo)
    {
        var approvalStatusPriorToDeletion = Approvals;
        IsDeleted = true;
        Publish(() => new CohortDeletedEvent(Id, EmployerAccountId, ProviderId, approvalStatusPriorToDeletion, DateTime.UtcNow));

        if (ChangeOfPartyRequestId.HasValue)
        {
            Publish(() => new CohortWithChangeOfPartyDeletedEvent(Id, ChangeOfPartyRequestId.Value, DateTime.UtcNow, deletedBy, userInfo));

            if (deletedBy == Party.Provider)
            {
                var firstName = DraftApprenticeships.First().FirstName;
                var lastName = DraftApprenticeships.First().LastName;
                Publish(() => new ProviderRejectedChangeOfPartyRequestEvent
                {
                    EmployerAccountId = EmployerAccountId,
                    EmployerName = AccountLegalEntity.Name,
                    TrainingProviderName = Provider.Name,
                    ChangeOfPartyRequestId = ChangeOfPartyRequestId.Value,
                    ApprenticeName = $"{firstName} {lastName}",
                    RecipientEmailAddress = LastUpdatedByEmployerEmail
                });
            }
        }
    }

    private void ResetTransferSenderRejection()
    {
        if (TransferApprovalStatus == Types.TransferApprovalStatus.Rejected)
        {
            TransferApprovalStatus = null;
            TransferApprovalActionedOn = null;
            LastAction = LastAction.AmendAfterRejected;
        }
    }

    private void CheckThereIsNoPendingTransferRequest()
    {
        if (TransferRequests.Any(x => x.Status == (byte)Types.TransferApprovalStatus.Pending))
        {
            throw new DomainException(nameof(TransferRequests), $"Cohort already has a pending transfer request");
        }
    }

    private void AddMessage(string text, Party sendingParty, UserInfo userInfo)
    {
        Messages.Add(new Message(this, sendingParty, userInfo.UserDisplayName, text ?? ""));
    }

    private void ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails, bool isContinuation, int maxAgeAtApprenticeshipStart)
    {
        var errors = draftApprenticeshipDetails.ValidateDraftApprenticeshipDetails(isContinuation, TransferSenderId, Apprenticeships, maxAgeAtApprenticeshipStart);
        errors.ThrowIfAny();
    }

    private void CheckIsWithParty(Party party)
    {
        if (party != WithParty)
        {
            throw new DomainException(nameof(WithParty), $"Cohort must be with the party; {party} is not valid");
        }
    }

    private void CheckIsCompleteForParty(Party party, bool apprenticeEmailRequired)
    {
        if (!DraftApprenticeships.Any())
        {
            throw new DomainException(nameof(Apprenticeships), $"Cohort must have at least one draft apprenticeship");
        }

        if (party is Party.Employer or Party.Provider && DraftApprenticeships.Any(x => !x.IsCompleteForParty(party, apprenticeEmailRequired)))
        {
            throw new DomainException(nameof(DraftApprenticeships), $"Cohort must be complete for {party}");
        }
    }

    private void CheckIsEmployer(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Party must be {Party.Employer}; {party} is not valid");
        }
    }

    private void CheckIsEmployerOrProvider(Party party)
    {
        if (party != Party.Employer && party != Party.Provider)
        {
            throw new DomainException(nameof(party), $"Party must be {Party.Employer} or {Party.Provider}; {party} is not valid");
        }
    }

    private void CheckIsEmployerOrProviderOrTransferSender(Party party)
    {
        if (party != Party.Employer && party != Party.Provider && party != Party.TransferSender)
        {
            throw new DomainException(nameof(party), $"Party must be {Party.Employer} or {Party.Provider} or {Party.TransferSender}; {party} is not valid");
        }
    }

    private void CheckDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails == null)
        {
            throw new DomainException(nameof(draftApprenticeshipDetails), "DraftApprenticeshipDetails must be supplied");
        }
    }

    private void UpdatedBy(Party party, UserInfo userInfo)
    {
        if (userInfo == null)
        {
            return;
        }

        switch (party)
        {
            case Party.Employer:
                LastUpdatedByEmployerName = userInfo.UserDisplayName;
                LastUpdatedByEmployerEmail = userInfo.UserEmail;
                break;
            case Party.Provider:
                LastUpdatedByProviderName = userInfo.UserDisplayName;
                LastUpdatedByProviderEmail = userInfo.UserEmail;
                break;
        }
    }

    public void RejectTransferRequest(UserInfo userInfo)
    {
        CheckIsWithParty(Party.TransferSender);
        StartTrackingSession(UserAction.RejectTransferRequest, Party.TransferSender, EmployerAccountId, ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);
        TransferApprovalStatus = Types.TransferApprovalStatus.Rejected;
        EditStatus = EditStatus.EmployerOnly;
        WithParty = Party.Employer;
        LastUpdatedOn = DateTime.UtcNow;
        ChangeTrackingSession.CompleteTrackingSession();
    }

    internal DraftApprenticeship GetDraftApprenticeship(long draftApprenticeshipId)
    {
        var existingDraftApprenticeship = DraftApprenticeships.SingleOrDefault(a => a.Id == draftApprenticeshipId);

        if (existingDraftApprenticeship == null)
        {
            throw new InvalidOperationException($"There is not a draft apprenticeship with id {draftApprenticeshipId} in cohort {Id}");
        }

        return existingDraftApprenticeship;
    }
}