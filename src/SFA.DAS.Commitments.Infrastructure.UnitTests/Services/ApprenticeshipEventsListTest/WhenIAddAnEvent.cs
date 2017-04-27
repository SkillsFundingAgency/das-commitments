using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsListTest
{
    [TestFixture]
    public class WhenIAddAnEvent
    {
        private ApprenticeshipEventsList _list;
        private Commitment _commitment;
        private Apprenticeship _apprenticeship;
        private string _event = "Test";
        private DateTime _effectiveFrom;

        [SetUp]
        public void Given()
        {
            _list = new ApprenticeshipEventsList();

            _commitment = new Commitment();
            _apprenticeship = new Apprenticeship();
            _effectiveFrom = DateTime.Now.AddDays(-10);
        }

        [Test]
        public void ThenTheEventIsAdded()
        {
            _list.Add(_commitment, _apprenticeship, _event, _effectiveFrom);

            _list.Events.Count.Should().Be(1);
            _list.Events.First().Apprenticeship.Should().Be(_apprenticeship);
            _list.Events.First().Commitment.Should().Be(_commitment);
            _list.Events.First().Event.Should().Be(_event);
            _list.Events.First().EffectiveFrom.Should().Be(_effectiveFrom);
        }
    }
}
