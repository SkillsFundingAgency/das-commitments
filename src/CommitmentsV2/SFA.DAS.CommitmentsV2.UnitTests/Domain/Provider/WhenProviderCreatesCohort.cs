using System;
using System.Linq;
using NUnit.Framework;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;
using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Provider
{
    [TestFixture]
    public class WhenProviderCreatesCohort
    {
        private class ProviderCreatesCohortTestFixture
        {
            public CommitmentsV2.Models.Provider Provider { get; private set; }
            public Commitment Cohort { get; private set; }
            public AccountLegalEntity AccountLegalEntity { get; private set; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; private set; }
            
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
               
                DraftApprenticeshipDetails = fixture.Create<DraftApprenticeshipDetails>();
            }

            public ProviderCreatesCohortTestFixture CreateCohort()
            {
                Cohort = Provider.CreateCohort(AccountLegalEntity, DraftApprenticeshipDetails);
                return this;
            }
        }

        [Test]
        public void TheCohortBelongsToTheProvider()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(fixture.Provider.UkPrn, fixture.Cohort.ProviderId);
        }

        [Test]
        public void TheCohortBelongsToTheGivenAccount()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(fixture.AccountLegalEntity.AccountId, fixture.Cohort.EmployerAccountId);
        }

        [Test]
        public void TheCohortBelongsToTheGivenAccountLegalEntity()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(fixture.AccountLegalEntity.LegalEntityId, fixture.Cohort.LegalEntityId);
        }

        [Test]
        public void TheCohortIsWithTheProvider()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(EditStatus.ProviderOnly, fixture.Cohort.EditStatus);
        }

        [Test]
        public void TheCohortIsADraft()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(LastAction.None, fixture.Cohort.LastAction);
        }

        [Test]
        public void TheCohortIsUnapproved()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            //approval is the aggregate of contained apprenticeship approvals, currently :-(
            Assert.IsTrue(fixture.Cohort.Apprenticeship.All(x => x.AgreementStatus == AgreementStatus.NotAgreed));
        }

        [Test]
        public void TheCohortHasOneDraftApprenticeship()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(1, fixture.Cohort.Apprenticeship.Count);
        }

        [Test]
        public void TheCohortIsProviderOriginated()
        {
            var fixture = new ProviderCreatesCohortTestFixture();
            fixture.CreateCohort();
            Assert.AreEqual(Originator.Provider, fixture.Cohort.Originator);
        }
    }
}
