using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Provider
{
    [TestFixture]
    public class WhenProviderCreatesCohort
    {
        private ProviderCreatesCohortTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ProviderCreatesCohortTestFixture();
        }

        [Test]
        public void TheCohortBelongsToTheProvider()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(_fixture.Provider.UkPrn, result.ProviderId);
        }

        [Test]
        public void TheCohortBelongsToTheGivenAccount()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(_fixture.AccountLegalEntity.AccountId, result.EmployerAccountId);
        }

        [Test]
        public void TheCohortBelongsToTheGivenAccountLegalEntity()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(_fixture.AccountLegalEntity.LegalEntityId, result.LegalEntityId);
        }

        [Test]
        public void TheCohortIsWithTheProvider()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(EditStatus.ProviderOnly, result.EditStatus);
        }

        [Test]
        public void TheCohortIsADraft()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(LastAction.None, result.LastAction);
        }

        [Test]
        public void TheCohortIsUnapproved()
        {
            var result = _fixture.CreateCohort();
            //approval is the aggregate of contained apprenticeship approvals, currently :-(
            Assert.IsTrue(result.Apprenticeship.All(x => x.AgreementStatus == AgreementStatus.NotAgreed));
        }

        [Test]
        public void TheCohortHasOneDraftApprenticeship()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(1, result.Apprenticeship.Count);
        }

        [Test]
        public void TheCohortIsPartyOriginated()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(_fixture.Party, result.Originator);
        }

        [Test]
        public void TheDraftApprenticeshipCreatedEventIsPublished()
        {
            var cohort = _fixture.CreateCohort();
            var draftApprenticeship = cohort.Apprenticeship.Single();

            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Cast<DraftApprenticeshipCreatedEvent>().Single().Should().BeEquivalentTo(new DraftApprenticeshipCreatedEvent(
                    cohortId: cohort.Id,
                    draftApprenticeshipId: draftApprenticeship.Id,
                    uln: draftApprenticeship.Uln,
                    reservationId: draftApprenticeship.ReservationId.Value,
                    createdOn: draftApprenticeship.CreatedOn.Value));
        }

        private class ProviderCreatesCohortTestFixture
        {
            public UnitOfWorkContext UnitOfWorkContext { get; private set; }
            public CommitmentsV2.Models.Provider Provider { get; private set; }
            public AccountLegalEntity AccountLegalEntity { get; private set; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
            public Originator Party { get; set; }

            public ProviderCreatesCohortTestFixture()
            {
                var fixture = new Fixture();

                UnitOfWorkContext = new UnitOfWorkContext();
                Provider = new CommitmentsV2.Models.Provider(fixture.Create<long>(), fixture.Create<string>(), fixture.Create<DateTime>(), fixture.Create<DateTime>());

                var account = new Account(fixture.Create<long>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<DateTime>());

                AccountLegalEntity = new AccountLegalEntity(account,
                    fixture.Create<long>(),
                    fixture.Create<string>(),
                    fixture.Create<string>(),
                    fixture.Create<string>(),
                    fixture.Create<OrganisationType>(),
                    fixture.Create<string>(),
                    fixture.Create<DateTime>());

                DraftApprenticeshipDetails = new DraftApprenticeshipDetails
                {
                    FirstName = fixture.Create<string>(),
                    LastName = fixture.Create<string>(),
                    ReservationId = Guid.NewGuid()
                };

                Party = Originator.Provider;
            }

            public Cohort CreateCohort()
            {
                var result = Provider.CreateCohort(AccountLegalEntity, DraftApprenticeshipDetails, Party);
                return result;
            }
        }
    }
}
