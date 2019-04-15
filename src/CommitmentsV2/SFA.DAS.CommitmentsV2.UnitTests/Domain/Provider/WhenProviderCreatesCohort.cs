using System;
using System.Linq;
using NUnit.Framework;
using AutoFixture;
using Moq;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;
using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Provider
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
        public void TheCohortIsProviderOriginated()
        {
            var result = _fixture.CreateCohort();
            Assert.AreEqual(Originator.Provider, result.Originator);
        }

        private class ProviderCreatesCohortTestFixture
        {
            public CommitmentsV2.Models.Provider Provider { get; private set; }
            public AccountLegalEntity AccountLegalEntity { get; private set; }
            private DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }

            public ProviderCreatesCohortTestFixture()
            {
                var fixture = new Fixture();

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
            }

            public Commitment CreateCohort()
            {
                var result = Provider.CreateCohort(AccountLegalEntity,
                    DraftApprenticeshipDetails,
                    Mock.Of<IUlnValidator>(),
                    Mock.Of<ICurrentDateTime>(),
                    Mock.Of<IAcademicYearDateProvider>());
                return result;
            }
        }
    }
}
