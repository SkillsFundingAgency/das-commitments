using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsListTest
{
    [TestFixture]
    public class WhenClearingEvents
    {
        private ApprenticeshipEventsList _list;
        private Commitment _commitment;
        private Apprenticeship _apprenticeship;
        private string _event = "Test";
        private DateTime _effectiveFrom;
        private DateTime _effectiveTo;

        [SetUp]
        public void Given()
        {
            _list = new ApprenticeshipEventsList();

            _commitment = new Commitment();
            _apprenticeship = new Apprenticeship();
            _effectiveFrom = DateTime.Now.AddDays(-10);
            _effectiveTo = DateTime.Now.AddDays(10);
        }

        [Test]
        public void ThenTheEventsAreCleared()
        {
            _list.Add(_commitment, _apprenticeship, _event, _effectiveFrom, _effectiveTo);
            _list.Add(_commitment, _apprenticeship, _event, _effectiveFrom, _effectiveTo);

            _list.Clear();

            _list.Events.Should().BeEmpty();
        }
    }
}
