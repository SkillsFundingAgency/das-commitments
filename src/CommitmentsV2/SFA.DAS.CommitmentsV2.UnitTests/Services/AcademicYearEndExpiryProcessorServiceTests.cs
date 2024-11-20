using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class AcademicYearEndExpiryProcessorServiceTests
{
    private Mock<ILogger<AcademicYearEndExpiryProcessorService>> _logger;
    private IAcademicYearDateProvider _academicYearProvider;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<IEventPublisher> _mockMessageBuilder;
    private Mock<ProviderCommitmentsDbContext> _dbContextMock;
    private Fixture _fixture;

    private AcademicYearEndExpiryProcessorService _sut;

    [SetUp]
    public void Arrange()
    {
        // ARRANGE
        _dbContextMock = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
        _logger = new Mock<ILogger<AcademicYearEndExpiryProcessorService>>();

        _currentDateTime = new Mock<ICurrentDateTime>();
        _academicYearProvider = new AcademicYearDateProvider(_currentDateTime.Object);
        _mockMessageBuilder = new Mock<IEventPublisher>();

        _currentDateTime.Setup(m => m.UtcNow).Returns(new DateTime(DateTime.Now.Year, 11, 1));

        _sut = new AcademicYearEndExpiryProcessorService(
            _logger.Object,
            _academicYearProvider,
            _dbContextMock.Object,
            _currentDateTime.Object,
            _mockMessageBuilder.Object);

        _fixture = new Fixture();

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(ApprenticeshipBase),
                typeof(Apprenticeship)));
    }

    [Test, MoqAutoData]
    public async Task WhenExpiring_ApprenticeshipUpdated_AndNoUpdatesFound_DoNothing()
    {
        _dbContextMock
            .Setup(context => context.ApprenticeshipUpdates)
            .ReturnsDbSet(new List<ApprenticeshipUpdate>());

        await _sut.ExpireApprenticeshipUpdates("jobId");

        _dbContextMock.Verify(m => m.SaveChanges(), Times.Never);
    }

    [Test]
    public async Task WhenExpiring_ApprenticeshipUpdated_WhenApprenticeshipUpdatesFound()
    {
        const int recordCount = 4;

        var testData = CreateApprenticeshipsUpdateExpiryTestData(recordCount);

        _dbContextMock
            .Setup(context => context.ApprenticeshipUpdates)
            .ReturnsDbSet(testData.apprenticeshipUpdates);

        _dbContextMock
            .Setup(context => context.Apprenticeships)
            .ReturnsDbSet(testData.apprenticeships);

        await _sut.ExpireApprenticeshipUpdates("jobId");

        _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(recordCount));

        testData.apprenticeshipUpdates.ForEach(update =>
        {
            var apprenticeship = testData.apprenticeships.Single(a => a.Id == update.ApprenticeshipId);
            _mockMessageBuilder.Verify(m =>
                    m.Publish(It.Is<ApprenticeshipUpdateCancelledEvent>(cancelled =>
                        cancelled.ApprenticeshipId == apprenticeship.Id &&
                        cancelled.AccountId == apprenticeship.Cohort.EmployerAccountId &&
                        cancelled.ProviderId == apprenticeship.Cohort.ProviderId)),
                "Should be called once for each update record, with correct params");
        });
    }

    [Test]
    public async Task WhenExpiring_ApprenticeshipUpdated_ShouldOnlyUpdateRecordsWithCostOrTrainingChanges()
    {
        int validRecordCount = 4;
        var validTestData = CreateApprenticeshipsUpdateExpiryTestData(validRecordCount);
        var testDataWithMissingCostOrTraining = CreateApprenticeshipsUpdateExpiryTestData(3, false);



        _dbContextMock
               .Setup(context => context.ApprenticeshipUpdates)
               .ReturnsDbSet(validTestData.apprenticeshipUpdates.Concat(testDataWithMissingCostOrTraining.apprenticeshipUpdates).ToList());

        _dbContextMock
            .Setup(context => context.Apprenticeships)
            .ReturnsDbSet(validTestData.apprenticeships.Concat(testDataWithMissingCostOrTraining.apprenticeships).ToList());

        await _sut.ExpireApprenticeshipUpdates("jobId");


        _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(validRecordCount));
    }

    [Test]
    public async Task WhenExpiring_Datalocks_AndNoDataFound_DoNothing()
    {
        _dbContextMock
            .Setup(context => context.DataLocks)
            .ReturnsDbSet(new List<DataLockStatus>());

        await _sut.ExpireDataLocks("jobId");

        _dbContextMock.Verify(m => m.SaveChanges(), Times.Never);
    }

    [Test]
    public async Task WhenExpiring_Datalocks_LogHowManyDlocksExpired()
    {
        var testData = CreateDatalockExpiryTestDate(5);

        _dbContextMock
            .Setup(context => context.DataLocks)
            .ReturnsDbSet(testData);

        await _sut.ExpireDataLocks("jobId");

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"expired {testData.Count} items")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Test]
    public async Task WhenExpiring_Datalocks_OnlyExpireThoseFromPreviousAcademicYears()
    {
        var testDataPreviousAcademicYears = CreateDatalockExpiryTestDate(5);
        var testDateAfterAcademicYear = CreateDatalockExpiryTestDate(7, true);

        _dbContextMock
            .Setup(context => context.DataLocks)
            .ReturnsDbSet(testDataPreviousAcademicYears.Concat(testDateAfterAcademicYear).ToList());

        await _sut.ExpireDataLocks("jobId");

        _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(5));
    }

    private DateTime GetPreviousAcademicYearDateTestValue(IFixture fixture)
    {
        return fixture
            .Create<Generator<DateTime>>()
            .First(s => s < _academicYearProvider.CurrentAcademicYearStartDate);
    }

    private DateTime GetCurrentAcademicYearDateTestValue(IFixture fixture)
    {
        return fixture
            .Create<Generator<DateTime>>()
            .First(s => s > _academicYearProvider.CurrentAcademicYearStartDate && s < _academicYearProvider.CurrentAcademicYearEndDate);
    }

    private (List<ApprenticeshipUpdate> apprenticeshipUpdates, List<Apprenticeship> apprenticeships) CreateApprenticeshipsUpdateExpiryTestData(int recordCount, bool withCostAndTraining = true)
    {
        var apprenticeships = new List<Apprenticeship>();

        var apprenticeshipUpdateComposer = _fixture
            .Build<ApprenticeshipUpdate>()
            .With(au => au.Status, ApprenticeshipUpdateStatus.Pending);

        if (!withCostAndTraining)
        {
            apprenticeshipUpdateComposer = apprenticeshipUpdateComposer
                .Without(au => au.Cost)
                .Without(au => au.TrainingCode)
                .Without(au => au.StartDate);
        }
        else
        {
            apprenticeshipUpdateComposer = apprenticeshipUpdateComposer
                .With(au => au.StartDate, GetPreviousAcademicYearDateTestValue(_fixture));
        }

        var apprenticeshipUpdates =
            apprenticeshipUpdateComposer
            .CreateMany(recordCount)
            .ToList();

        apprenticeshipUpdates.ForEach(update =>
        {
            var apprenticeship = _fixture.Build<Apprenticeship>()
                .With(a => a.Id, update.ApprenticeshipId)
                .With(a => a.StartDate, GetPreviousAcademicYearDateTestValue(_fixture))
                .Create();

            apprenticeships.Add(apprenticeship);
            update.Apprenticeship = apprenticeship;
        });

        return (apprenticeshipUpdates, apprenticeships);
    }

    private List<DataLockStatus> CreateDatalockExpiryTestDate(int recordCount, bool afterAcademicYear = false)
    {
        var dataLocks = _fixture
            .Build<DataLockStatus>()
            .With(dl => dl.IsExpired, false)
            .With(dl => dl.Expired, (DateTime?)null)
            .With(dl => dl.IlrEffectiveFromDate, afterAcademicYear ? GetCurrentAcademicYearDateTestValue(_fixture) : GetPreviousAcademicYearDateTestValue(_fixture))
            .CreateMany(recordCount)
            .ToList();

        return dataLocks;
    }
}
