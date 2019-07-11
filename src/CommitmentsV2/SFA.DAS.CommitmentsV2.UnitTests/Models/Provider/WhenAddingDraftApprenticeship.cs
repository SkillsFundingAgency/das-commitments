using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Provider
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
                .And.Subject.Cast<DraftApprenticeshipCreatedEvent>().Single().Should().BeEquivalentTo(new DraftApprenticeshipCreatedEvent(
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
                .SetParty(Originator.Provider)
                .AddDraftApprenticeship();

            draftApprenticeship.Should().NotBeNull().And.Match<DraftApprenticeship>(d =>
                d.EmployerRef == null &&
                d.ProviderRef == _fixture.DraftApprenticeshipDetails.Reference);
        }

        [Test]
        public void AndEditStatusIsEmployerOnlyAndPartyIsEmployerThenShouldReturnDraftApprenticeship()
        {
            var draftApprenticeship = _fixture.SetEditStatus(EditStatus.EmployerOnly)
                .SetParty(Originator.Employer)
                .AddDraftApprenticeship();

            draftApprenticeship.Should().NotBeNull().And.Match<DraftApprenticeship>(d =>
                d.EmployerRef == _fixture.DraftApprenticeshipDetails.Reference &&
                d.ProviderRef == null);
        }

        private class AddDraftApprenticeshipTestFixture
        {
            public DateTime Now { get; set; }
            public Fixture Fixture { get; set; }
            public Cohort Cohort { get; set; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
            public Originator Party { get; set; }
            public UnitOfWorkContext UnitOfWorkContext { get; set; }
            
            public AddDraftApprenticeshipTestFixture()
            {
                Now = DateTime.UtcNow;
                Fixture = new Fixture();

                Cohort = Fixture.Build<Cohort>()
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

                Party = Originator.Provider;
                UnitOfWorkContext = new UnitOfWorkContext();
            }

            public DraftApprenticeship AddDraftApprenticeship()
            {
                return Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Party);
            }

            public AddDraftApprenticeshipTestFixture SetEditStatus(EditStatus editStatus)
            {
                Cohort.Set(c => c.EditStatus, editStatus);

                return this;
            }

            public AddDraftApprenticeshipTestFixture SetParty(Originator party)
            {
                Party = party;

                return this;
            }
        }
    }
}