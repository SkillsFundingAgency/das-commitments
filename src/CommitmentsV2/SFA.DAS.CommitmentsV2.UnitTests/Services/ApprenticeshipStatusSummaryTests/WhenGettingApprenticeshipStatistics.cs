using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.ApprenticeshipStatusSummaryTests
{
    [TestFixture]
    public class WhenGettingApprenticeshipStatistics
    {
        private Fixture _fixture;
        private Random _random;
        private int _lastNumberOfDays;

        private Mock<ILogger<ApprenticeshipStatusSummaryService>> _loggerMock;
        private ProviderCommitmentsDbContext _context;
        private Lazy<ProviderCommitmentsDbContext> _providerCommitmentsDbContextMock;

        private ApprenticeshipStatusSummaryService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _random = new Random();
            _lastNumberOfDays = _fixture.Create<int>();

            _loggerMock = new Mock<ILogger<ApprenticeshipStatusSummaryService>>();
            _context = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase("TestDb", new InMemoryDatabaseRoot())
                .Options);
            _providerCommitmentsDbContextMock = new Lazy<ProviderCommitmentsDbContext>(_context);

            _sut = new ApprenticeshipStatusSummaryService(_providerCommitmentsDbContextMock, _loggerMock.Object);
        }

        [Test]
        public async Task ThenCorrectlyReturnsApprovedCount()
        {
            //Arrange
            var expectedApprovedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, true, default, 10);
            var unexpectedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, false, default, 7);

            //Act
            var result = await _sut.GetApprenticeshipStatisticsFor(_lastNumberOfDays);

            //Assert
            result.ApprovedApprenticeshipCount.Should().Be(expectedApprovedApprenticeships.Count);
        }

        [Test]
        public async Task ThenCorrectlyReturnsPausedCount()
        {
            //Arrange
            var expectedPausedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, true, PaymentStatus.Paused, 12);
            var unexpectedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, false, PaymentStatus.Paused, 8);

            //Act
            var result = await _sut.GetApprenticeshipStatisticsFor(_lastNumberOfDays);

            //Assert
            result.PausedApprenticeshipCount.Should().Be(expectedPausedApprenticeships.Count);
        }

        [Test]
        public async Task ThenCorrectlyReturnsStoppedCount()
        {
            //Arrange
            var expectedStoppedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, true, PaymentStatus.Withdrawn, 6);
            var unexpectedApprenticeships = await AddApprenticeshipsToDb(_lastNumberOfDays, false, PaymentStatus.Withdrawn, 4);

            //Act
            var result = await _sut.GetApprenticeshipStatisticsFor(_lastNumberOfDays);

            //Assert
            result.StoppedApprenticeshipCount.Should().Be(expectedStoppedApprenticeships.Count);
        }

        private async Task<List<Apprenticeship>> AddApprenticeshipsToDb(int lastNumberOfDays, bool isExpectedInCount, PaymentStatus paymentStatus, int amountToCreate)
        {
            var fromDate = DateTime.UtcNow.AddDays(Math.Abs(lastNumberOfDays) * -1).Date;
            var testFromDate = isExpectedInCount ? fromDate.AddDays(1) : fromDate.AddDays(-1);

            var apprenticeships = new List<Apprenticeship>();

            for (int i = 0; i < amountToCreate; i++)
            {
                var randomNum = _random.Next(2);
                var apprenticeship = new Apprenticeship();

                var cohort = new Cohort();
                cohort.EmployerAndProviderApprovedOn = testFromDate;
                cohort.Approvals = randomNum == 1 ? (Party)3 : (Party)7;

                apprenticeship.Cohort = cohort;
                apprenticeship.CommitmentId = cohort.Id;
                apprenticeship.StopDate = testFromDate;
                apprenticeship.PauseDate = testFromDate;
                apprenticeship.PaymentStatus = paymentStatus;
                apprenticeship.IsApproved = true;

                apprenticeships.Add(apprenticeship);
            }

            _context.Apprenticeships.AddRange(apprenticeships);

            await _context.SaveChangesAsync();

            return apprenticeships.ToList();
        }
    }
}