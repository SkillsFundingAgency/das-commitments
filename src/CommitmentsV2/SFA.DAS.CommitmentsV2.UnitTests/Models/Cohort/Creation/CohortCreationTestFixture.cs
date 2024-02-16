using System;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.Creation
{
    public class CohortCreationTestFixture
    {
        private readonly Fixture _autoFixture = new Fixture();
        public Party CreatingParty { get; private set; }
        public CommitmentsV2.Models.Cohort Cohort { get; private set; }
        public CommitmentsV2.Models.Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public Account TransferSender { get; private set; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; private set; }
        public Exception Exception { get; private set; }
        public UnitOfWorkContext UnitOfWorkContext { get; private set; }
        public UserInfo UserInfo { get; }
        public long? TransferSenderId { get; }
        public int? PledgeApplicationId { get; }
        public string TransferSenderName { get; }

        public CohortCreationTestFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();

            Provider = new CommitmentsV2.Models.Provider(_autoFixture.Create<long>(), _autoFixture.Create<string>(),
                _autoFixture.Create<DateTime>(), _autoFixture.Create<DateTime>());

            var account = new Account(_autoFixture.Create<long>(), _autoFixture.Create<string>(),
                _autoFixture.Create<string>(), _autoFixture.Create<string>(), _autoFixture.Create<DateTime>());
            UserInfo = _autoFixture.Create<UserInfo>();

            AccountLegalEntity = new AccountLegalEntity(account,
                _autoFixture.Create<long>(),
                _autoFixture.Create<long>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<OrganisationType>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<DateTime>());

            TransferSenderId = _autoFixture.Create<long>();
            TransferSenderName = _autoFixture.Create<string>();
            TransferSender = new Account(TransferSenderId.Value, "XXX", "ZZZ", TransferSenderName, new DateTime());
            PledgeApplicationId = _autoFixture.Create<int?>();
        }

        public CohortCreationTestFixture WithCreatingParty(Party creatingParty)
        {
            CreatingParty = creatingParty;
            return this;
        }

        public CohortCreationTestFixture WithDraftApprenticeship(string email = null)
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                FirstName = _autoFixture.Create<string>(),
                LastName = _autoFixture.Create<string>(),
                ReservationId = Guid.NewGuid(),
                Email = email,
                DeliveryModel = DeliveryModel.Regular,
                IsOnFlexiPaymentPilot = false
            };

            return this;
        }

        public CohortCreationTestFixture WithNoTransferSender()
        {
            TransferSender = null;
            return this;
        }

        public void CreateCohort()
        {
            Exception = null;

            try
            {
                Cohort = new CommitmentsV2.Models.Cohort(Provider.UkPrn,
                    AccountLegalEntity.AccountId,
                    AccountLegalEntity.Id,
                    TransferSender?.Id,
                    PledgeApplicationId,
                    DraftApprenticeshipDetails,
                    CreatingParty,
                    UserInfo);

                Cohort.TransferSender = TransferSender;
            }
            catch (ArgumentException ex)
            {
                Exception = ex;
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public CohortCreationTestFixture WithInvalidStartDate()
        {
            DraftApprenticeshipDetails.StartDate = new DateTime(1000, 1, 1);
            return this;
        }

        public void VerifyOriginator(Originator expectedOriginator)
        {
            Assert.That(Cohort.Originator, Is.EqualTo(expectedOriginator));
        }

        public void VerifyCohortIsUnapproved()
        {
            Assert.That(Cohort.Approvals == Party.None, Is.True);
        }

        public void VerifyCohortContainsDraftApprenticeship()
        {
            Assert.That(Cohort.Apprenticeships.Any(), Is.True);
        }


        public void VerifyNoMessageIsAdded()
        {
            Assert.That(Cohort.Messages.Any(), Is.False);
        }

        public void VerifyException<T>()
        {
            Assert.That(Exception, Is.Not.Null);
            Assert.That(Exception, Is.InstanceOf<T>());
        }

        public void VerifyNoException()
        {
            Assert.That(Exception, Is.Null);
        }

        public void VerifyCohortIsDraft()
        {
            Assert.That(Cohort.IsDraft, Is.True);
        }

        public void VerifyCohortBelongsToAccount()
        {
            Assert.That(Cohort.EmployerAccountId, Is.EqualTo(AccountLegalEntity.AccountId));
        }

        public void VerifyCohortBelongsToAccountLegalEntity()
        {
            Assert.That(Cohort.AccountLegalEntityId, Is.EqualTo(AccountLegalEntity.Id));
        }

        public void VerifyCohortHasTransferInformation()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Cohort.TransferSenderId, Is.EqualTo(TransferSenderId));
                Assert.That(Cohort.TransferSender.Name, Is.EqualTo(TransferSenderName));
            });
        }

        public void VerifyCohortHasNoTransferInformation()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Cohort.TransferSenderId, Is.Null);
                Assert.That(Cohort.TransferSender, Is.Null);
            });
        }

        public void VerifyCohortHasPledgeApplicationId()
        {
            Assert.That(Cohort.PledgeApplicationId, Is.EqualTo(PledgeApplicationId));
        }

        public void VerifyCohortBelongsToProvider()
        {
            Assert.That(Cohort.ProviderId, Is.EqualTo(Provider.UkPrn));
        }

        public void VerifyLastUpdatedFieldsAreSetForParty(Party modifyingParty)
        {
            switch (modifyingParty)
            {
                case Party.Employer:
                    Assert.That(Cohort.LastUpdatedByEmployerName, Is.EqualTo(UserInfo.UserDisplayName));
                    Assert.That(Cohort.LastUpdatedByEmployerEmail, Is.EqualTo(UserInfo.UserEmail));
                    break;
                case Party.Provider:
                    Assert.That(Cohort.LastUpdatedByProviderName, Is.EqualTo(UserInfo.UserDisplayName));
                    Assert.That(Cohort.LastUpdatedByProviderEmail, Is.EqualTo(UserInfo.UserEmail));
                    break;
            }
        }

        public void VerifyDraftApprenticeshipCreatedEventIsPublished()
        {
            var draftApprenticeship = Cohort.Apprenticeships.Single();

            UnitOfWorkContext.GetEvents().OfType<DraftApprenticeshipCreatedEvent>().Should().ContainSingle(e =>
                    e.CohortId == Cohort.Id &&
                    e.DraftApprenticeshipId == draftApprenticeship.Id &&
                    e.Uln == draftApprenticeship.Uln &&
                    e.ReservationId == draftApprenticeship.ReservationId &&
                    e.CreatedOn == draftApprenticeship.CreatedOn);
        }

        public void VerifyCohortIsWithCreator()
        {
            Assert.That(Cohort.EditStatus, Is.EqualTo(CreatingParty.ToEditStatus()));
        }

        public void VerifyCohortTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                  && @event.EntityType ==
                                                                  nameof(Cohort)), Is.Not.Null);
        }

        public void VerifyDraftApprenticeshipTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                  && @event.EntityType ==
                                                                  nameof(DraftApprenticeship)), Is.Not.Null);
        }
    }
}