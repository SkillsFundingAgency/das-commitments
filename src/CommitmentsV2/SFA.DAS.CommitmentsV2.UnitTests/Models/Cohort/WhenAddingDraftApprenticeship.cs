using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using MoreLinq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    [Parallelizable]
    public class WhenAddingDraftApprenticeship
    {
        private AddDraftApprenticeshipTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddDraftApprenticeshipTestFixture();
        }

        [Test]
        public void ThenShouldAddDraftApprenticeshipToCohort()
        {
            var draftApprenticeship = _fixture.AddDraftApprenticeship();

            draftApprenticeship.Should().BeSameAs(_fixture.Cohort.Apprenticeships.SingleOrDefault());
        }

        [Test]
        public void ThenShouldPublishDraftApprenticeshipCreatedEvent()
        {
            var draftApprenticeship = _fixture.AddDraftApprenticeship();

            _fixture.UnitOfWorkContext.GetEvents().Should().HaveCount(1)
                .And.Subject.Cast<DraftApprenticeshipCreatedEvent>().Single().Should().BeEquivalentTo(
                    new DraftApprenticeshipCreatedEvent(
                        cohortId: _fixture.Cohort.Id,
                        draftApprenticeshipId: draftApprenticeship.Id,
                        uln: _fixture.DraftApprenticeshipDetails.Uln,
                        reservationId: draftApprenticeship.ReservationId.Value,
                        createdOn: draftApprenticeship.CreatedOn.Value));
        }

        [Test]
        public void ThenShouldReturnDraftApprenticeship()
        {
            var draftApprenticeship = _fixture.AddDraftApprenticeship();

            draftApprenticeship.Should().NotBeNull().And.Match<DraftApprenticeship>(d =>
                d.FirstName == _fixture.DraftApprenticeshipDetails.FirstName &&
                d.LastName == _fixture.DraftApprenticeshipDetails.LastName &&
                d.Uln == _fixture.DraftApprenticeshipDetails.Uln &&
                d.ProgrammeType == _fixture.DraftApprenticeshipDetails.TrainingProgramme.ProgrammeType &&
                d.CourseCode == _fixture.DraftApprenticeshipDetails.TrainingProgramme.CourseCode &&
                d.Cost == _fixture.DraftApprenticeshipDetails.Cost &&
                d.StartDate == _fixture.DraftApprenticeshipDetails.StartDate &&
                d.EndDate == _fixture.DraftApprenticeshipDetails.EndDate &&
                d.DateOfBirth == _fixture.DraftApprenticeshipDetails.DateOfBirth &&
                d.ReservationId == _fixture.DraftApprenticeshipDetails.ReservationId);
        }

        [Test]
        public void AndEditStatusIsProviderOnlyAndPartyIsProviderThenShouldReturnDraftApprenticeship()
        {
            var draftApprenticeship = _fixture.SetEditStatus(EditStatus.ProviderOnly)
                .SetParty(Party.Provider)
                .AddDraftApprenticeship();

            draftApprenticeship.Should().NotBeNull().And.Match<DraftApprenticeship>(d =>
                d.EmployerRef == null &&
                d.ProviderRef == _fixture.DraftApprenticeshipDetails.Reference);
        }

        [Test]
        public void AndEditStatusIsEmployerOnlyAndPartyIsEmployerThenShouldReturnDraftApprenticeship()
        {
            var draftApprenticeship = _fixture.SetEditStatus(EditStatus.EmployerOnly)
                .SetParty(Party.Employer)
                .AddDraftApprenticeship();

            draftApprenticeship.Should().NotBeNull().And.Match<DraftApprenticeship>(d =>
                d.EmployerRef == _fixture.DraftApprenticeshipDetails.Reference &&
                d.ProviderRef == null);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void ThenTheApprovalOfTheOtherPartyIsUndone(Party modifyingParty)
        {
            _fixture
                .WithExistingDraftApprenticeship()
                .WithApproval(modifyingParty.GetOtherParty())
                .AddDraftApprenticeship();

            Assert.IsTrue(_fixture.Cohort.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.NotAgreed));
        }

        private class AddDraftApprenticeshipTestFixture
        {
            public DateTime Now { get; set; }
            public Fixture Fixture { get; set; }
            public CommitmentsV2.Models.Cohort Cohort { get; set; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
            public DraftApprenticeship ExistingApprenticeshipDetails { get; set; }
            public Party Party { get; set; }
            public UnitOfWorkContext UnitOfWorkContext { get; set; }

            public UserInfo UserInfo { get; }

            public AddDraftApprenticeshipTestFixture()
            {
                Now = DateTime.UtcNow;
                Fixture = new Fixture();

                Cohort = Fixture.Build<CommitmentsV2.Models.Cohort>()
                    .OmitAutoProperties()
                    .With(c => c.Id)
                    .With(c => c.EditStatus, EditStatus.ProviderOnly)
                    .Create();

                DraftApprenticeshipDetails = Fixture.Build<DraftApprenticeshipDetails>()
                    .Without(d => d.StartDate)
                    .Without(d => d.EndDate)
                    .Without(d => d.DateOfBirth)
                    .Without(d => d.Uln)
                    .Create();

                ExistingApprenticeshipDetails = new DraftApprenticeship(Fixture.Build<DraftApprenticeshipDetails>().Create(), Party.Provider);

                Party = Party.Provider;
                UnitOfWorkContext = new UnitOfWorkContext();
                UserInfo = Fixture.Create<UserInfo>();
            }

            public DraftApprenticeship AddDraftApprenticeship()
            {
                return Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Party, UserInfo);
            }

            public AddDraftApprenticeshipTestFixture SetEditStatus(EditStatus editStatus)
            {
                Cohort.Set(c => c.EditStatus, editStatus);

                return this;
            }

            public AddDraftApprenticeshipTestFixture SetParty(Party party)
            {
                Party = party;

                return this;
            }

            public AddDraftApprenticeshipTestFixture WithExistingDraftApprenticeship()
            {
                Cohort.Apprenticeships.Add(ExistingApprenticeshipDetails);
                return this;
            }

            public AddDraftApprenticeshipTestFixture WithApproval(Party approvingParty)
            {
                var agreementStatus = AgreementStatus.NotAgreed;

                switch (approvingParty)
                {
                    case Party.Employer:
                        agreementStatus = AgreementStatus.EmployerAgreed;
                        break;
                    case Party.Provider:
                        agreementStatus = AgreementStatus.ProviderAgreed;
                        break;
                    default:
                        break;
                }

                Cohort.Apprenticeships.ForEach(a => a.AgreementStatus = agreementStatus);

                return this;
            }
        }
    }
}