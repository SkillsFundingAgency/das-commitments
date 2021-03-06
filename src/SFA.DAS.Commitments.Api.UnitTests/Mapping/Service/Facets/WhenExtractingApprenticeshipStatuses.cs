﻿using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Mapping.Service.Facets
{
    [TestFixture]
    public class WhenExtractingApprenticeshipStatuses
    {
        private FacetMapper _sut;
        private List<Apprenticeship> _data;
        private ApprenticeshipSearchQuery _userQuery;
        private Mock<ICurrentDateTime> _currentDateTime;

        [SetUp]
        public void SetUp()
        {
            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.Setup(x => x.Now).Returns(new DateTime(2018, 3, 1));

            _data = new List<Apprenticeship>();

            _userQuery = new ApprenticeshipSearchQuery();
            _sut = new FacetMapper(_currentDateTime.Object);
        }

        [Test]
        public void ShouldOnlyCreateOneFacetPerStatus()
        {
            _data.Add(new Apprenticeship
            {
                FirstName = "Pending approval",
                PaymentStatus = PaymentStatus.Active,
                StartDate = _currentDateTime.Object.Now.AddMonths(1)
            });

            _data.Add(new Apprenticeship
            {
                FirstName = "Pending approval",
                PaymentStatus = PaymentStatus.Active,
                StartDate = _currentDateTime.Object.Now.AddMonths(1)
            });

            var result = _sut.BuildFacets(_data, _userQuery, Originator.Provider);

            result.ApprenticeshipStatuses.Count.Should().Be(1);
            AssertStatus(result, ApprenticeshipStatus.WaitingToStart, 1);
            AssertStatus(result, ApprenticeshipStatus.Live, 0);
            AssertStatus(result, ApprenticeshipStatus.Paused, 0);
            AssertStatus(result, ApprenticeshipStatus.Stopped, 0);
            AssertStatus(result, ApprenticeshipStatus.Finished, 0);
        }

        [Test]
        public void ShouldHaveWaitingForStopped()
        {
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Withdrawn });
            _data.Add(new Apprenticeship { FirstName = "Started", PaymentStatus = PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(-1) });

            _userQuery.ApprenticeshipStatuses = new List<ApprenticeshipStatus> { ApprenticeshipStatus.Stopped };

            var result = _sut.BuildFacets(_data, _userQuery, Originator.Provider);

            var selected = result.ApprenticeshipStatuses.Where(m => m.Selected);
            selected.Count().Should().Be(1);
            selected.Single().Data.Should().Be(ApprenticeshipStatus.Stopped);

            result.ApprenticeshipStatuses.Count.Should().Be(2);

            AssertStatus(result, ApprenticeshipStatus.WaitingToStart, 0);
            AssertStatus(result, ApprenticeshipStatus.Live, 1);
            AssertStatus(result, ApprenticeshipStatus.Paused, 0);
            AssertStatus(result, ApprenticeshipStatus.Stopped, 1);
            AssertStatus(result, ApprenticeshipStatus.Finished, 0);
        }

        [Test]
        public void ShouldHaveFacetsSelected_When_WaitingForStoppedAndLive()
        {
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Withdrawn });
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Completed });
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(-1) });

            _userQuery.ApprenticeshipStatuses = new List<ApprenticeshipStatus> { ApprenticeshipStatus.Stopped, ApprenticeshipStatus.Live };

            var result = _sut.BuildFacets(_data, _userQuery, Originator.Provider);

            var selected = result.ApprenticeshipStatuses.Where(m => m.Selected);
            selected.Count().Should().Be(2);
            selected.Any(m => m.Data == ApprenticeshipStatus.Live).Should().BeTrue();
            selected.Any(m => m.Data == ApprenticeshipStatus.Stopped).Should().BeTrue();

            result.ApprenticeshipStatuses.Count.Should().Be(3);

            AssertStatus(result, ApprenticeshipStatus.WaitingToStart, 0);
            AssertStatus(result, ApprenticeshipStatus.Live, 1);
            AssertStatus(result, ApprenticeshipStatus.Paused, 0);
            AssertStatus(result, ApprenticeshipStatus.Stopped, 1);
            AssertStatus(result, ApprenticeshipStatus.Finished, 1);
        }

        [Test]
        public void ShouldHaveFacetsSelected_OnlyIfProviderHasApprenticeshipWithThatStatus()
        {
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Withdrawn });
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Completed });
            _data.Add(new Apprenticeship { PaymentStatus = PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddDays(30) });

            _userQuery.ApprenticeshipStatuses = new List<ApprenticeshipStatus> { ApprenticeshipStatus.Stopped, ApprenticeshipStatus.Live };

            var result = _sut.BuildFacets(_data, _userQuery, Originator.Provider);

            var selected = result.ApprenticeshipStatuses.Where(m => m.Selected);
            selected.Count().Should().Be(1);
            selected.Any(m => m.Data == ApprenticeshipStatus.Stopped).Should().BeTrue();
            selected.Any(m => m.Data == ApprenticeshipStatus.Live).Should().BeFalse();

            result.ApprenticeshipStatuses.Count.Should().Be(3);

            AssertStatus(result, ApprenticeshipStatus.WaitingToStart, 1);
            AssertStatus(result, ApprenticeshipStatus.Live, 0);
            AssertStatus(result, ApprenticeshipStatus.Paused, 0);
            AssertStatus(result, ApprenticeshipStatus.Stopped, 1);
            AssertStatus(result, ApprenticeshipStatus.Finished, 1);
        }

        private void AssertStatus(Api.Types.Apprenticeship.Facets result, ApprenticeshipStatus status, int i)
        {
            result.ApprenticeshipStatuses.Count(m => m.Data == status).Should().Be(i);
        }
    }
}
