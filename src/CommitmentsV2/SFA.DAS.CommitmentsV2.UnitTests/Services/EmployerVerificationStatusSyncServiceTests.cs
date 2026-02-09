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
        await _sut.SyncPendingEmploymentChecksAsync();

        _apiClient.Verify(
            x => x.Get<GetEmploymentChecksResponse>(It.IsAny<IGetApiRequest>()),
            Times.Never);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenPending_UpdatesFromEvsCheckResult()
    {
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

        await _sut.SyncPendingEmploymentChecksAsync();

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

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Failed);
    }

    [Test]
    public async Task SyncPendingEmploymentChecksAsync_WhenSkippedOrErrorCode_MapsToErrorAndNotes()
    {
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

        await _sut.SyncPendingEmploymentChecksAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(apprenticeshipId);
        request.Should().NotBeNull();
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

        await _sut.Invoking(s => s.SyncPendingEmploymentChecksAsync()).Should().NotThrowAsync();

        var request = await _db.EmployerVerificationRequests.FindAsync(200L);
        request!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
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
