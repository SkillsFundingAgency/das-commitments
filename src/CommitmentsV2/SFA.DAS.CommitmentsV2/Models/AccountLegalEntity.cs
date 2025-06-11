#nullable enable
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class AccountLegalEntity : ICohortOriginator
{
    public long Id { get; private set; }
    public string LegalEntityId { get; private set; }
    public long MaLegalEntityId { get; private set; }
    public string PublicHashedId { get; private set; }
    public virtual Account Account { get; private set; }
    public long AccountId { get; private set; }
    public string Name { get; private set; }
    public OrganisationType OrganisationType { get; private set; }
    public string? Address { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }
    public DateTime? Deleted { get; private set; }

    public virtual ICollection<Cohort> Cohorts { get; set; }
    public virtual ICollection<ChangeOfPartyRequest> ChangeOfPartyRequests { get; set; }

    public AccountLegalEntity(Account account, long id, long maLegalEntityId, string legalEntityId, string publicHashedId,
        string name, OrganisationType organisationType, string address, DateTime created)
    {
        Id = id;
        LegalEntityId = legalEntityId;
        MaLegalEntityId = maLegalEntityId;
        PublicHashedId = publicHashedId;
        Account = account;
        AccountId = account.Id;
        Name = name;
        OrganisationType = organisationType;
        Address = address;
        Created = created;
        Deleted = null;
    }

    public AccountLegalEntity()
    {
    }

    public void UpdateName(string name, DateTime updated)
    {
        if (IsUpdatedNameDateChronological(updated) && IsUpdatedNameDifferent(name))
        {
            EnsureAccountLegalEntityHasNotBeenDeleted();

            Name = name;
            Updated = updated;
        }
    }

    internal void Delete(DateTime deleted)
    {
        EnsureAccountLegalEntityHasNotBeenDeleted();
        Deleted = deleted;
    }

    private void EnsureAccountLegalEntityHasNotBeenDeleted()
    {
        if (Deleted != null)
        {
            throw new InvalidOperationException("Requires account legal entity has not been deleted");
        }
    }

    private bool IsUpdatedNameDateChronological(DateTime updated)
    {
        return (Updated == null || updated > Updated.Value) && (Deleted == null || updated > Deleted.Value);
    }

    private bool IsUpdatedNameDifferent(string name)
    {
        return name != Name;
    }

    public virtual Cohort CreateCohort(long providerId, AccountLegalEntity accountLegalEntity, Account transferSender, int? pledgeApplicationId,
        DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, int maximumAgeAtApprenticeshipStart = Constants.MaximumAgeAtApprenticeshipStart)
    {
        return new Cohort(providerId, accountLegalEntity.AccountId, accountLegalEntity.Id, transferSender?.Id, pledgeApplicationId, draftApprenticeshipDetails, Party.Employer, userInfo, maximumAgeAtApprenticeshipStart);
    }

    public Cohort CreateCohort(long providerId, AccountLegalEntity accountLegalEntity, UserInfo userInfo)
    {
        throw new NotImplementedException();
    }

    public virtual Cohort CreateCohortWithOtherParty(long providerId, AccountLegalEntity accountLegalEntity, Account transferSender, int? pledgeApplicationId, string message, UserInfo userInfo)
    {
        return new Cohort(providerId, accountLegalEntity.AccountId, accountLegalEntity.Id, transferSender?.Id, pledgeApplicationId, Party.Employer, message, userInfo);
    }
}