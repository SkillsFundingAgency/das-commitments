﻿using System;
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
        }

        public CohortCreationTestFixture WithCreatingParty(Party creatingParty)
        {
            CreatingParty = creatingParty;
            return this;
        }

        public CohortCreationTestFixture WithDraftApprenticeship()
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                FirstName = _autoFixture.Create<string>(),
                LastName = _autoFixture.Create<string>(),
                ReservationId = Guid.NewGuid()
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
                Cohort = new CommitmentsV2.Models.Cohort(Provider,
                    AccountLegalEntity,
                    TransferSender,
                    DraftApprenticeshipDetails,
                    CreatingParty,
                    UserInfo);
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
            Assert.AreEqual(expectedOriginator, Cohort.Originator);
        }

        public void VerifyCohortIsUnapproved()
        {
            //approval is the aggregate of contained apprenticeship approvals, currently :-(
            Assert.IsTrue(Cohort.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.NotAgreed));
        }

        public void VerifyCohortContainsDraftApprenticeship()
        {
            Assert.IsTrue(Cohort.Apprenticeships.Any());
        }


        public void VerifyNoMessageIsAdded()
        {
            Assert.IsFalse(Cohort.Messages.Any());
        }

        public void VerifyException<T>()
        {
            Assert.IsNotNull(Exception);
            Assert.IsInstanceOf<T>(Exception);
        }

        public void VerifyNoException()
        {
            Assert.IsNull(Exception);
        }

        public void VerifyCohortIsDraft()
        {
            Assert.AreEqual(LastAction.None, Cohort.LastAction);
        }

        public void VerifyCohortBelongsToLegalEntity()
        {
            Assert.AreEqual(AccountLegalEntity.LegalEntityId, Cohort.LegalEntityId);
        }

        public void VerifyCohortBelongsToAccount()
        {
            Assert.AreEqual(AccountLegalEntity.AccountId, Cohort.EmployerAccountId);
        }

        public void VerifyCohortHasTransferInformation()
        {
            Assert.AreEqual(TransferSenderId, Cohort.TransferSenderId);
            Assert.AreEqual(TransferSenderName, Cohort.TransferSenderName);
        }

        public void VerifyCohortHasNoTransferInformation()
        {
            Assert.IsNull(Cohort.TransferSenderId);
            Assert.IsNull(Cohort.TransferSenderName);
        }

        public void VerifyCohortBelongsToProvider()
        {
            Assert.AreEqual(Provider.UkPrn, Cohort.ProviderId);
        }

        public void VerifyLastUpdatedFieldsAreSetForParty(Party modifyingParty)
        {
            switch (modifyingParty)
            {
                case Party.Employer:
                    Assert.AreEqual(UserInfo.UserDisplayName, Cohort.LastUpdatedByEmployerName);
                    Assert.AreEqual(UserInfo.UserEmail, Cohort.LastUpdatedByEmployerEmail);
                    break;
                case Party.Provider:
                    Assert.AreEqual(UserInfo.UserDisplayName, Cohort.LastUpdatedByProviderName);
                    Assert.AreEqual(UserInfo.UserEmail, Cohort.LastUpdatedByProviderEmail);
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
            Assert.AreEqual(CreatingParty.ToEditStatus(), Cohort.EditStatus);
        }

        public void VerifyCohortTracking()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                  && @event.EntityType ==
                                                                  nameof(Cohort)));
        }

        public void VerifyDraftApprenticeshipTracking()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                  && @event.EntityType ==
                                                                  nameof(DraftApprenticeship)));
        }
    }
}