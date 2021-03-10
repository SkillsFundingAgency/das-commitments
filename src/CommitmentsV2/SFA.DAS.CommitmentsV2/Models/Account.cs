using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Account : Aggregate, ITrackableEntity
    {
        public long Id { get; private set; }
        public string HashedId { get; private set; }
        public string PublicHashedId { get; private set; }
        public string Name { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime? Updated { get; private set; }
        public ApprenticeshipEmployerType LevyStatus { get; set; }
        public virtual ICollection<AccountLegalEntity> AccountLegalEntities => _accountLegalEntities;

        private readonly List<AccountLegalEntity> _accountLegalEntities = new List<AccountLegalEntity>();

        public virtual ICollection<Cohort> TransferFundedCohorts { get; set; }

        public virtual ICollection<CustomProviderPaymentPriority> CustomProviderPaymentPriorities { get; set; }

        public Account(long id, string hashedId, string publicHashedId, string name, DateTime created)
        {
            Id = id;
            HashedId = hashedId;
            PublicHashedId = publicHashedId;
            Name = name;
            Created = created;
        }

        public Account()
        {
        }

        public AccountLegalEntity AddAccountLegalEntity(long accountLegalEntityId, long maLegalEntityId, string legalEntityId,
            string accountLegalEntityPublicHashedId, string name, OrganisationType organisationType, string address,
            DateTime added)
        {
            EnsureAccountLegalEntityHasNotAlreadyBeenAdded(accountLegalEntityId);

            var accountLegalEntity = new AccountLegalEntity(this, accountLegalEntityId, maLegalEntityId, legalEntityId,
                accountLegalEntityPublicHashedId, name, organisationType, address, added);

            _accountLegalEntities.Add(accountLegalEntity);

            return accountLegalEntity;
        }

        public void UpdateName(string name, DateTime updated)
        {
            if (IsUpdatedNameDateChronological(updated) && IsUpdatedNameDifferent(name))
            {
                Name = name;
                Updated = updated;
            }
        }

        public void UpdateLevyStatus(ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            LevyStatus = apprenticeshipEmployerType;
        }

        public void RemoveAccountLegalEntity(AccountLegalEntity accountLegalEntity, DateTime removed)
        {
            EnsureAccountLegalEntityHasBeenAdded(accountLegalEntity);

            accountLegalEntity.Delete(removed);
        }

        public void AddCustomProviderPaymentPriority(Func<CustomProviderPaymentPriority> add, UserInfo userInfo)
        {
            var item = add();
            StartTrackingSession(UserAction.UpdateCustomProviderPaymentPriorities, Party.Employer, Id, item.ProviderId, userInfo);
            ChangeTrackingSession.TrackInsert(item);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public void RemoveCustomProviderPaymentPriority(Func<CustomProviderPaymentPriority> remove, UserInfo userInfo)
        {
            var item = remove();
            StartTrackingSession(UserAction.UpdateCustomProviderPaymentPriorities, Party.Employer, Id, item.ProviderId, userInfo);
            ChangeTrackingSession.TrackDelete(item);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public void UpdateCustomProviderPaymentPriority(long providerId, int priorityOrder, UserInfo userInfo)
        {
            CustomProviderPaymentPriorities
                .Single(w => w.ProviderId == providerId)
                .UpdateProviderPriority(priorityOrder, userInfo);
        }

        public void NotifyCustomProviderPaymentPrioritiesChanged()
        {
            var paymentOrder = CustomProviderPaymentPriorities.OrderBy(x => x.PriorityOrder).Select(x => (int)x.ProviderId);
            Publish(() => new PaymentOrderChangedEvent
            {
                AccountId = Id,
                PaymentOrder = paymentOrder.ToArray()
            });
        }

        private void EnsureAccountLegalEntityHasBeenAdded(AccountLegalEntity accountLegalEntity)
        {
            if (_accountLegalEntities.All(ale => ale.Id != accountLegalEntity.Id))
            {
                throw new InvalidOperationException("Requires account legal entity has been added");
            }
        }

        private void EnsureAccountLegalEntityHasNotAlreadyBeenAdded(long accountLegalEntityId)
        {
            if (_accountLegalEntities.Any(ale => ale.Id == accountLegalEntityId))
            {
                throw new InvalidOperationException("Requires account legal entity has not already been added");
            }
        }

        private bool IsUpdatedNameDateChronological(DateTime updated)
        {
            return Updated == null || updated > Updated.Value;
        }

        private bool IsUpdatedNameDifferent(string name)
        {
            return name != Name;
        }
    }
}