using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public sealed class OverlappingTrainingDateRequestDomainServiceTests
    {
        private IFixture Fixture = new Fixture();
        private Mock<ProviderCommitmentsDbContext> _dbContext;
        private Mock<IOverlapCheckService> _overlapCheckServiceMock;
        private Mock<ICurrentDateTime> _currentDateTimeMock;
        private OverlappingTrainingDateRequestDomainService _sut;

        [SetUp]
        public void Setup()
        {
            new UnitOfWorkContext();

            _currentDateTimeMock = new Mock<ICurrentDateTime>();
            _overlapCheckServiceMock = new Mock<IOverlapCheckService>();

            Fixture.Customizations.Add(
                new TypeRelay(
                    typeof(ApprenticeshipBase),
                    typeof(Apprenticeship)));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _dbContext =
                new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options)
                { CallBase = true };

            _sut = new OverlappingTrainingDateRequestDomainService(
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object), _overlapCheckServiceMock.Object,
                _currentDateTimeMock.Object);
        }

        [Test]
        [InlineAutoData(Party.Employer)]
        [InlineAutoData(Party.None)]
        [InlineAutoData(Party.TransferSender)]
        public void WhenPartyIsNotProvider_ThenThrowDomainException(
            Party party,
            UserInfo userInfo,
            long apprenticeshipId
        )
        {
            // assert
            Assert.ThrowsAsync<DomainException>(async () =>
                await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, party, null, userInfo,
                    new CancellationToken()));
        }

        [Test, MoqAutoData]
        public void WhenPartyIsProvider_AndApprenticeshipIdInvalid_ThrowException(
            UserInfo userInfo,
            long apprenticeshipId
        )
        {
            // arrange 
            var draftApprenticeships = Fixture
                .Build<DraftApprenticeship>()
                .With(app => app.Id, Fixture.Create<Generator<long>>().Where(l => l != apprenticeshipId).First())
                .CreateMany();

            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            // assert
            Assert.ThrowsAsync<BadRequestException>(async () =>
                await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null, userInfo,
                    new CancellationToken()));
        }

        [Test, MoqAutoData]
        public void WhenCohortIsApproved_ThrowException(
            UserInfo userInfo,
            long apprenticeshipId
        )
        {
            // arrange
            var draftApprenticeships = GetDraftApprenticeshipTestData(apprenticeshipId, true);
            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            // assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null, userInfo,
                    new CancellationToken()));
        }

        [Test]
        [InlineAutoData(false, true, true)]
        [InlineAutoData(true, false, true)]
        [InlineAutoData(true, true, false)]
        public void WheRequiredFieldsAreEmpty_ThenThrowException(
            bool setUln,
            bool setStartDate,
            bool setEndDate,
            UserInfo userInfo,
            long apprenticeshipId
        )
        {
            // arrange
            var draftApprenticeships = GetDraftApprenticeshipTestData(apprenticeshipId, false);
            draftApprenticeships[0].Uln = setUln ? draftApprenticeships[0].Uln : null;
            draftApprenticeships[0].StartDate = setStartDate ? draftApprenticeships[0].StartDate : null;
            draftApprenticeships[0].EndDate = setEndDate ? draftApprenticeships[0].EndDate : null;

            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            // assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null, userInfo,
                    new CancellationToken()));
        }

        [Test, MoqAutoData]
        public void WhenOverlapNoLongerExists_ThrowException(
            UserInfo userInfo,
            long apprenticeshipId,
            string uln
        )
        {
            // arrange
            var draftApprenticeships = GetDraftApprenticeshipTestData(apprenticeshipId, false);
            draftApprenticeships[0].Id = apprenticeshipId;
            draftApprenticeships[0].Uln = uln;

            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            _overlapCheckServiceMock
                .Setup(m => m.CheckForOverlapsOnStartDate(uln, It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(),
                    apprenticeshipId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OverlapCheckResultOnStartDate(false, null));

            // assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null, userInfo,
                    new CancellationToken()));
        }

        [Test, MoqAutoData]
        public async Task WhenOverlapExists_OverlapRequestCreated(
            UserInfo userInfo,
            long apprenticeshipId,
            long previousApprenticeshipId,
            string uln
        )
        {
            // arrange
            var now = DateTime.UtcNow;
            _currentDateTimeMock.Setup(m => m.UtcNow).Returns(now);

            var draftApprenticeships = GetDraftApprenticeshipTestData(apprenticeshipId, false);
            draftApprenticeships[0].Id = apprenticeshipId;
            draftApprenticeships[0].Uln = uln;

            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            _overlapCheckServiceMock
                .Setup(m => m.CheckForOverlapsOnStartDate(uln, It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(),
                    apprenticeshipId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OverlapCheckResultOnStartDate(true, previousApprenticeshipId));

            // act
            var result = await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null,
                userInfo, new CancellationToken());

            // assert
            result.CreatedOn.Should().Be(now);
        }

        [Test, MoqAutoData]
        public async Task WhenOverlapExists_OverlapRequestCommittedToDb(
            UserInfo userInfo,
            long apprenticeshipId,
            long previousApprenticeshipId,
            string uln
        )
        {
            // arrange
            var now = DateTime.UtcNow;
            _currentDateTimeMock.Setup(m => m.UtcNow).Returns(now);

            var draftApprenticeships = GetDraftApprenticeshipTestData(apprenticeshipId, false);
            draftApprenticeships[0].Id = apprenticeshipId;
            draftApprenticeships[0].Uln = uln;

            _dbContext.Setup(db => db.DraftApprenticeships).ReturnsDbSet(draftApprenticeships);

            _overlapCheckServiceMock
                .Setup(m => m.CheckForOverlapsOnStartDate(uln, It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(),
                    apprenticeshipId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OverlapCheckResultOnStartDate(true, previousApprenticeshipId));

            // act
            var result = await _sut.CreateOverlappingTrainingDateRequest(apprenticeshipId, Party.Provider, null,
                userInfo, new CancellationToken());

            // assert
            _dbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private List<DraftApprenticeship> GetDraftApprenticeshipTestData(long testApprenticeshipId,
            bool isCohortApproved)
        {
            var cohort = Fixture
                .Build<Cohort>()
                .With(c => c.WithParty, isCohortApproved ? Party.None : Party.Provider)
                .Create();

            var draftApprenticeships = Fixture
                .Build<DraftApprenticeship>()
                .CreateMany()
                .ToList();

            draftApprenticeships[0].Id = testApprenticeshipId;
            draftApprenticeships[0].Cohort = cohort;

            return draftApprenticeships;
        }
    }
}