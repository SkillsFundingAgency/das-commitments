using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class EmployerVerificationStatusSyncServiceTests
{
    private ProviderCommitmentsDbContext _db;
    private Mock<IApprovalsOuterApiClient> _apiClient;
    private EmployerVerificationStatusSyncService _sut;

    [SetUp]
    public void SetUp()
    {
        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options);

        _apiClient = new Mock<IApprovalsOuterApiClient>();
        _sut = new EmployerVerificationStatusSyncService(
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            _apiClient.Object,
            Mock.Of<ILogger<EmployerVerificationStatusSyncService>>());
    }

    [TearDown]
    public void TearDown()
    {
        _db?.Database.EnsureDeleted();
        _db?.Dispose();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenNoPending_DoesNotCallApi()
    {
        // Act
        await _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        _apiClient.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenPending_UpdatesFromEvsCheckResult()
    {
        // Arrange
        const long apprenticeshipId = 100;
        SeedPendingRequest(apprenticeshipId);

        var dateOfCheck = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var checks = new List<EvsCheckResponse>
        {
            new()
            {
                ApprenticeshipId = apprenticeshipId,
                DateOfCheck = dateOfCheck,
                Result = new EvsCheckResult { CompletionStatus = 2, Employed = true }
            }
        };

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = checks });

        // Act
        await _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Passed);
        request.LastCheckedDate.Should().Be(dateOfCheck);
        request.Updated.Should().NotBeNull();
        request.Notes.Should().BeNull();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenEmployedFalse_MapsToFailed()
    {
        // Arrange
        const long apprenticeshipId = 101;
        SeedPendingRequest(apprenticeshipId);

        var checks = new List<EvsCheckResponse>
        {
            new()
            {
                ApprenticeshipId = apprenticeshipId,
                DateOfCheck = DateTime.UtcNow,
                Result = new EvsCheckResult { CompletionStatus = 2, Employed = false }
            }
        };

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = checks });

        // Act
        await _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Failed);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenSkippedOrErrorCode_MapsToErrorAndNotes()
    {
        // Arrange
        const long apprenticeshipId = 102;
        SeedPendingRequest(apprenticeshipId);

        var checks = new List<EvsCheckResponse>
        {
            new()
            {
                ApprenticeshipId = apprenticeshipId,
                DateOfCheck = DateTime.UtcNow,
                Result = new EvsCheckResult { CompletionStatus = 3, ErrorCode = "NinoNotFound" }
            }
        };

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = checks });

        // Act
        await _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Error);
        request.Notes.Should().Be("NinoNotFound");
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenApiReturnsNull_DoesNotThrow()
    {
        // Arrange
        SeedPendingRequest(200);

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync((GetEmploymentChecksResponse)null);

        // Act
        var act = () => _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        act.Should().NotThrowAsync();
        var request = await _db.EmployerVerificationRequests.FindAsync(200L);
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenMoreThan50Pending_MakesPagedApiCalls()
    {
        // Arrange: 150 pending, ApiPageSize 50 => 3 API calls
        for (var i = 1; i <= 150; i++)
        {
            SeedPendingRequest(i);
        }

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = [] });

        // Act
        await _sut.SyncPendingEmploymentChecksAsync();

        // Assert
        _apiClient.Verify(
            x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()),
            Times.Exactly(3));
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenCreatedLessThanOneDayAgo_DoesNotSelectRecord()
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = 300,
            Created = DateTime.UtcNow.AddHours(-12),
            Status = EmployerVerificationRequestStatus.Pending
        });
        _db.SaveChanges();

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenCreatedMoreThanFiveMonthsAgo_DoesNotSelectRecord()
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = 301,
            Created = DateTime.UtcNow.AddMonths(-6),
            Updated = null,
            Status = EmployerVerificationRequestStatus.Pending
        });
        _db.SaveChanges();

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenStatusPassedAndUpdatedOverOneDayAgo_DoesNotSelectForPeriodicRecheck()
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = 302,
            Created = DateTime.UtcNow.AddMonths(-1),
            Updated = DateTime.UtcNow.AddDays(-2),
            Status = EmployerVerificationRequestStatus.Passed
        });
        _db.SaveChanges();

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenStatusFailedAndUpdatedOverOneDayAgo_SelectsForPeriodicRecheck()
    {
        const long apprenticeshipId = 303;
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = apprenticeshipId,
            Created = DateTime.UtcNow.AddMonths(-1),
            Updated = DateTime.UtcNow.AddDays(-2),
            Status = EmployerVerificationRequestStatus.Failed
        });
        _db.SaveChanges();

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = [] });

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.Verify(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()), Times.Once);
    }

    private void SeedPendingRequest(long apprenticeshipId)
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = apprenticeshipId,
            Created = DateTime.UtcNow.AddDays(-1),
            Status = EmployerVerificationRequestStatus.Pending
        });
        _db.SaveChanges();
    }
}
