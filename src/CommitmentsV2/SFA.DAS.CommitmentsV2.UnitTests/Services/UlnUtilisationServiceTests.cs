using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class UlnUtilisationServiceTests
    {
        private UlnUtilisationServiceFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new UlnUtilisationServiceFixture();
        }

        [Test]
        public async Task WhenCalculatingOverlapEndDate_AndApprenticeshipCompletedBeforeEndDate_ThenCompletionDateUsed()
        {
            var expectedUln = _fixture.CompletedApprenticeshipBeforeEndDate.Uln;
            var expectedEndDate = _fixture.CompletedApprenticeshipBeforeEndDate.CompletionDate.Value;

            _fixture
                .WithApprenticeshipsInDb();

            var result = await _fixture.Act(expectedUln);

            _fixture.Assert_CorrectEndDateWasUsed(result, expectedUln, expectedEndDate);
        }

        [Test]
        public async Task WhenCalculatingOverlapEndDate_AndApprenticeshipCompletedAfterEndDate_ThenEndDateIsUsed()
        {
            var expectedUln = _fixture.CompletedApprenticeshipAfterEndDate.Uln;
            var expectedEndDate = _fixture.CompletedApprenticeshipAfterEndDate.EndDate.Value;

            _fixture
                .WithApprenticeshipsInDb();

            var result = await _fixture.Act(expectedUln);

            _fixture.Assert_CorrectEndDateWasUsed(result, expectedUln, expectedEndDate);
        }

        [Test]
        public async Task WhenCalculatingOverlapEndDate_AndApprenticeshipWithdrawn_ThenStopDateUsed()
        {
            var expectedUln = _fixture.WithdrawnApprenticeship.Uln;
            var expectedEndDate = _fixture.WithdrawnApprenticeship.StopDate.Value;

            _fixture
                .WithApprenticeshipsInDb();

            var result = await _fixture.Act(expectedUln);

            _fixture.Assert_CorrectEndDateWasUsed(result, expectedUln, expectedEndDate);
        }

        [Test]
        public async Task WhenCalculatingEndDate_AndApprenticeshipLive_ThenEndDateIsUsed()
        {
            var expectedUln = _fixture.LiveApprenticeship.Uln;
            var expectedEndDate = _fixture.LiveApprenticeship.EndDate.Value;

            _fixture
                .WithApprenticeshipsInDb();

            var result = await _fixture.Act(expectedUln);

            _fixture.Assert_CorrectEndDateWasUsed(result, expectedUln, expectedEndDate);
        }
    }

    public class UlnUtilisationServiceFixture
    {
        public Apprenticeship CompletedApprenticeshipBeforeEndDate { get; }
        public Apprenticeship CompletedApprenticeshipAfterEndDate { get; }
        public Apprenticeship WithdrawnApprenticeship { get; }
        public Apprenticeship LiveApprenticeship { get; }

        private readonly ProviderCommitmentsDbContext _dbContext;
        private readonly Mock<IDbContextFactory> _iDbContextFactoryMock;
        private readonly UlnUtilisationService _sut;

        public UlnUtilisationServiceFixture()
        {
            CompletedApprenticeshipBeforeEndDate = new Apprenticeship
            {
                Id = 100,
                Uln = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Completed,
                CompletionDate = DateTime.UtcNow.AddMonths(-1),
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow
            };

            CompletedApprenticeshipAfterEndDate = new Apprenticeship
            {
                Id = 200,
                Uln = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Completed,
                CompletionDate = DateTime.UtcNow.AddMonths(1),
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow
            };

            WithdrawnApprenticeship = new Apprenticeship
            {
                Id = 300,
                Uln = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Withdrawn,
                StopDate = DateTime.UtcNow.AddMonths(-1),
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow
            };

            LiveApprenticeship = new Apprenticeship
            {
                Id = 400,
                Uln = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow
            };

            _dbContext = TestHelper.GetInMemoryDatabase();

            _iDbContextFactoryMock = new Mock<IDbContextFactory>();
            _iDbContextFactoryMock
                .Setup(x => x.CreateDbContext())
                .Returns(_dbContext);

            _sut = new UlnUtilisationService(_iDbContextFactoryMock.Object);
        }

        public Task<UlnUtilisation[]> Act(string uln) => _sut.GetUlnUtilisations(uln, default(CancellationToken));

        public UlnUtilisationServiceFixture WithApprenticeshipsInDb()
        {
            _dbContext.Apprenticeships.Add(CompletedApprenticeshipBeforeEndDate);
            _dbContext.Apprenticeships.Add(CompletedApprenticeshipAfterEndDate);
            _dbContext.Apprenticeships.Add(WithdrawnApprenticeship);
            _dbContext.Apprenticeships.Add(LiveApprenticeship);
            _dbContext.SaveChanges();
            return this;
        }

        public void Assert_CorrectEndDateWasUsed(UlnUtilisation[] result, string expectedUln, DateTime expectedEndDate)
        {
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(expectedUln,result.First().Uln);
            Assert.AreEqual(expectedEndDate, result.First().DateRange.To);
        }
    }
}