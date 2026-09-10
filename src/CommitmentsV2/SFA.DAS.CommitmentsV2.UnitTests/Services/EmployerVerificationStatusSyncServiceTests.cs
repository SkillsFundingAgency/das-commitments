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
        request!.Employed.Should().BeTrue();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Passed);
        request.LastCheckedDate.Should().Be(dateOfCheck);
        request.Updated.Should().NotBeNull();
        request.Notes.Should().BeNull();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenEmployedTrueAndCompletionStatusStarted_MapsToPassed()
    {
        const long apprenticeshipId = 105;
        SeedPendingRequest(apprenticeshipId);

        var checks = new List<EvsCheckResponse>
        {
            new()
            {
                ApprenticeshipId = apprenticeshipId,
                DateOfCheck = DateTime.UtcNow,
                Result = new EvsCheckResult { CompletionStatus = 1, Employed = true }
            }
        };

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = checks });

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Employed.Should().BeTrue();
        request.Status.Should().Be(EmployerVerificationRequestStatus.Passed);
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
        request!.Employed.Should().BeFalse();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Failed);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenEmployedFalseAndCompletionStatusStarted_MapsToFailed()
    {
        const long apprenticeshipId = 106;
        SeedPendingRequest(apprenticeshipId);

        var checks = new List<EvsCheckResponse>
        {
            new()
            {
                ApprenticeshipId = apprenticeshipId,
                DateOfCheck = DateTime.UtcNow,
                Result = new EvsCheckResult { CompletionStatus = 1, Employed = false }
            }
        };

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = checks });

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Employed.Should().BeFalse();
        request.Status.Should().Be(EmployerVerificationRequestStatus.Failed);
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
        request!.Employed.Should().BeNull();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Error);
        request.Notes.Should().Be("NinoNotFound");
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenApiReturnsNull_DoesNotThrow()
    {
        SeedPendingRequest(200);

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync((GetEmploymentChecksResponse)null);

        var act = () => _sut.SyncPendingEmploymentChecksAsync();

        await act.Should().NotThrowAsync();
        var request = await _db.EmployerVerificationRequests.FindAsync(200L);
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
        request.Updated.Should().NotBeNull();
        request.Employed.Should().BeNull();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenApiReturnsNoMatchingCheck_StillSetsUpdatedSoRecordLeavesTheNeverPolledQueue()
    {
        const long apprenticeshipId = 210;
        SeedPendingRequest(apprenticeshipId);

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = [] });

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
        request.Employed.Should().BeNull();
        request.Updated.Should().NotBeNull();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenApiReturnsCheckForOnlyOneId_UnmatchedIdIsStillMarkedUpdated()
    {
        SeedPendingRequest(220);
        SeedPendingRequest(221);

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse
            {
                Checks =
                [
                    new EvsCheckResponse
                    {
                        ApprenticeshipId = 220,
                        DateOfCheck = DateTime.UtcNow,
                        Result = new EvsCheckResult { CompletionStatus = 2, Employed = true }
                    }
                ]
            });

        await _sut.SyncPendingEmploymentChecksAsync();

        var matched = await _db.EmployerVerificationRequests.FindAsync(220L);
        matched!.Status.Should().Be(EmployerVerificationRequestStatus.Passed);
        matched.Employed.Should().BeTrue();
        matched.Updated.Should().NotBeNull();

        var unmatched = await _db.EmployerVerificationRequests.FindAsync(221L);
        unmatched!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
        unmatched.Employed.Should().BeNull();
        unmatched.Updated.Should().NotBeNull();
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenNoCheckFound_StillRechecksDailyUntilFiveMonths()
    {
        const long apprenticeshipId = 230;
        SeedPendingRequest(apprenticeshipId);

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = [] });

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request!.Updated.Should().NotBeNull();
        request.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
        request.Employed.Should().BeNull();

        request.Updated = DateTime.UtcNow.AddDays(-2);
        await _db.SaveChangesAsync();
        _apiClient.Invocations.Clear();

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.Verify(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()), Times.Once);
        request.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        request.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenPendingInsideFiveMonthWindowAndUpdatedYesterday_IsSelected()
    {
        const long apprenticeshipId = 231;
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = apprenticeshipId,
            Created = DateTime.UtcNow.AddMonths(-4),
            Updated = DateTime.UtcNow.AddDays(-1),
            Employed = null,
            Status = EmployerVerificationRequestStatus.Pending
        });
        _db.SaveChanges();

        _apiClient
            .Setup(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()))
            .ReturnsAsync(new GetEmploymentChecksResponse { Checks = [] });

        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.Verify(x => x.Get<GetEmploymentChecksResponse>(It.IsAny<GetEmploymentChecksRequest>()), Times.Once);
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
    public async Task SyncPendingEmploymentChecksAsync_WhenEmployedTrueAndUpdatedOverOneDayAgo_DoesNotSelectForPeriodicRecheck()
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = 302,
            Created = DateTime.UtcNow.AddMonths(-1),
            Updated = DateTime.UtcNow.AddDays(-2),
            Employed = true,
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

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenStatusPassedButEmployedUnknownAndUpdatedOverOneDayAgo_SelectsForPeriodicRecheck()
    {
        _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = 304,
            Created = DateTime.UtcNow.AddMonths(-1),
            Updated = DateTime.UtcNow.AddDays(-2),
            Employed = null,
            Status = EmployerVerificationRequestStatus.Passed
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
