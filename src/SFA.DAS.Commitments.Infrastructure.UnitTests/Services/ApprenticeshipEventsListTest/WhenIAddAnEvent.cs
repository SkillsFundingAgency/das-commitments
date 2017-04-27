using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsListTest
{
    [TestFixture]
    public class WhenIAddAnEvent
    {
        private ApprenticeshipEventsList _list;
        private Commitment _commitment;
        private Apprenticeship _apprenticeship;
        private string _event = "Test";

        [SetUp]
        public void Given()
        {
            _list = new ApprenticeshipEventsList();

            _commitment = new Commitment
            {
                Id = 348957,
                ProviderId = 123,
                EmployerAccountId = 987,
                LegalEntityId = "LE ID",
                LegalEntityName = "LE Name",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse
            };

            _apprenticeship = new Apprenticeship
            {
                EndDate = DateTime.Now.AddYears(3),
                StartDate = DateTime.Now.AddDays(1),
                Cost = 123.45m,
                TrainingCode = "TRCODE",
                AgreementStatus = Domain.Entities.AgreementStatus.BothAgreed,
                Id = 34875,
                ULN = "ULN",
                PaymentStatus = Domain.Entities.PaymentStatus.Active,
                TrainingType = TrainingType.Framework,
                PaymentOrder = 213,
                DateOfBirth = DateTime.Now.AddYears(-18)
            };
        }

        [Test]
        public void AndTheTrainingTypeIsFrameworkThenTheEventIsAddedWithTheCorrectTrainingType()
        {
            _apprenticeship.TrainingType = TrainingType.Framework;

            _list.Add(_commitment, _apprenticeship, _event);

            AssertEventWasAdded(_event);
        }

        [Test]
        public void AndTheTrainingTypeIsStandardThenTheEventIsAddedWithTheCorrectTrainingType()
        {
            _apprenticeship.TrainingType = TrainingType.Standard;

            _list.Add(_commitment, _apprenticeship, _event);

            AssertEventWasAdded(_event);
        }

        [Test]
        public void AndTheEffectiveFromDateIsProvidedThenTheEventIsAddedWithTheCorrectEffectiveFromDate()
        {
            _apprenticeship.TrainingType = TrainingType.Standard;
            var effectiveFrom = DateTime.Now.AddDays(-1);

            _list.Add(_commitment, _apprenticeship, _event, effectiveFrom);

            AssertEventWasAdded(_event, effectiveFrom);
        }

        private void AssertEventWasAdded(string @event, DateTime? effectiveFrom = null)
        {
            _list.Events.Count.Should().Be(1);
            AssertEventMatchesParameters(_list.Events[0], @event, effectiveFrom);
        }

        private void AssertEventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event, DateTime? effectiveFrom)
        {
            apprenticeshipEvent.AgreementStatus.Should().Be((Events.Api.Types.AgreementStatus)_apprenticeship.AgreementStatus);
            apprenticeshipEvent.ApprenticeshipId.Should().Be(_apprenticeship.Id);
            apprenticeshipEvent.EmployerAccountId.Should().Be(_commitment.EmployerAccountId.ToString());
            apprenticeshipEvent.Event.Should().Be(@event);
            apprenticeshipEvent.LearnerId.Should().Be((_apprenticeship.ULN ?? "NULL"));
            apprenticeshipEvent.TrainingId.Should().Be(_apprenticeship.TrainingCode);
            apprenticeshipEvent.PaymentStatus.Should().Be((Events.Api.Types.PaymentStatus)_apprenticeship.PaymentStatus);
            apprenticeshipEvent.ProviderId.Should().Be(_commitment.ProviderId.ToString());
            apprenticeshipEvent.TrainingEndDate.Should().Be(_apprenticeship.EndDate);
            apprenticeshipEvent.TrainingStartDate.Should().Be(_apprenticeship.StartDate);
            apprenticeshipEvent.TrainingTotalCost.Should().Be(_apprenticeship.Cost);
            apprenticeshipEvent.TrainingType.Should().Be((_apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard));
            apprenticeshipEvent.PaymentOrder.Should().Be(_apprenticeship.PaymentOrder);
            apprenticeshipEvent.LegalEntityId.Should().Be(_commitment.LegalEntityId);
            apprenticeshipEvent.LegalEntityName.Should().Be(_commitment.LegalEntityName);
            apprenticeshipEvent.LegalEntityOrganisationType.Should().Be(_commitment.LegalEntityOrganisationType.ToString());
            apprenticeshipEvent.DateOfBirth.Should().Be(_apprenticeship.DateOfBirth);
            apprenticeshipEvent.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }
}
