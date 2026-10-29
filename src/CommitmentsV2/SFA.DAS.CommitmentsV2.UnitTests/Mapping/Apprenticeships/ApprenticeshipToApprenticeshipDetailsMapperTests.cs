using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships;

public class ApprenticeshipToApprenticeshipDetailsMapperTests
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_To_ApprenticeshipDetails(
        Apprenticeship source,
        decimal cost,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.PriceHistory = new List<PriceHistory>{new PriceHistory
        {
            ApprenticeshipId = source.Id,
            Cost = cost,
            ToDate = null,
            FromDate = DateTime.UtcNow.AddMonths(-1)
        }};
        source.AgreedOn = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Unspecified);

        var result = await mapper.Map(source);

        result.Id.Should().Be(source.Id);
        result.FirstName.Should().Be(source.FirstName);
        result.LastName.Should().Be(source.LastName);
        result.Email.Should().Be(source.Email);
        result.CourseName.Should().Be(source.CourseName);
        result.EmployerName.Should().Be(source.Cohort.AccountLegalEntity.Name);
        result.ProviderName.Should().Be(source.Cohort.Provider.Name);
        result.StartDate.Should().Be(source.StartDate.Value);
        result.EndDate.Should().Be(source.EndDate.Value);
        result.PauseDate.Should().Be(source.PauseDate.Value);
        result.AgreedOn.Should().Be(source.AgreedOn.Value);
        result.PaymentStatus.Should().Be(source.PaymentStatus);
        result.Uln.Should().Be(source.Uln);
        result.DateOfBirth.Should().Be(source.DateOfBirth.Value);
        result.ProviderRef.Should().Be(source.ProviderRef);
        result.EmployerRef.Should().Be(source.EmployerRef);
        result.TotalAgreedPrice.Should().Be(cost);
        result.CohortReference.Should().Be(source.Cohort.Reference);
        result.AccountLegalEntityId.Should().Be(source.Cohort.AccountLegalEntityId);
        result.ActualStartDate.Should().Be(source.ActualStartDate);
        result.EmployerHasEditedCost.Should().Be(source.EmployerHasEditedCost);
        result.TrainingCourseVersion.Should().Be(source.TrainingCourseVersion);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Uses_Cohort_Approved_Date_When_Apprenticeship_AgreedOn_Is_Null(
        Apprenticeship source,
        decimal cost,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        // Arrange
        var approvedOn = new DateTime(2024, 7, 1, 12, 0, 0, DateTimeKind.Unspecified);
        source.AgreedOn = null;
        source.Cohort.EmployerAndProviderApprovedOn = approvedOn;
        source.PriceHistory = new List<PriceHistory>
        {
            new()
            {
                ApprenticeshipId = source.Id,
                Cost = cost,
                ToDate = null,
                FromDate = DateTime.UtcNow.AddMonths(-1)
            }
        };

        // Act
        var result = await mapper.Map(source);

        // Assert
        result.AgreedOn.Should().Be(approvedOn);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_IlrMismatch_When_Course_DataLock(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.Unknown,
                IsResolved = false,
                ErrorCode = DataLockErrorCode.Dlock03
            }
        };
        source.ApprenticeshipUpdate.Clear();
        source.OverlappingTrainingDateRequests.Clear();
        source.ApprovalRequests = new List<ApprovalRequest>();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.IlrDataMismatch);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_IlrMismatch_When_Price_DataLock(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = true;
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.Unknown,
                IsResolved = false,
                ErrorCode = DataLockErrorCode.Dlock07
            }
        };
        source.ApprenticeshipUpdate.Clear(); // isprovidesearch true
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.IlrDataMismatch);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesPending_When_Course_DataLock_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.Change,
                IsResolved = false,
                ErrorCode = DataLockErrorCode.Dlock03
            }
        };
        source.ApprenticeshipUpdate.Clear();
        source.OverlappingTrainingDateRequests.Clear();
        source.ApprovalRequests = new List<ApprovalRequest>();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesPending);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesPending_When_Price_DataLock_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.Change,
                IsResolved = false,
                ErrorCode = DataLockErrorCode.Dlock07
            }
        };
        source.ApprenticeshipUpdate.Clear();
        source.OverlappingTrainingDateRequests.Clear();
        source.ApprovalRequests = new List<ApprovalRequest>();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesPending);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesRequested_When_Course_DataLock(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.Restart,
                IsResolved = false,
                ErrorCode = DataLockErrorCode.Dlock04
            }
        };
        source.ApprenticeshipUpdate.Clear();
        source.OverlappingTrainingDateRequests.Clear();
        source.ApprovalRequests = new List<ApprovalRequest>();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesRequested);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_When_EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.DataLockStatus = new List<DataLockStatus>
        {
            new DataLockStatus
            {
                TriageStatus = TriageStatus.FixIlr,
                IsResolved = false,
                Status = Status.Fail
            }
        };
        source.IsProviderSearch = false;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate.Clear();
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesRequested);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_No_Apprenticeship_Alerts_When_No_ApprenticeshipUpdate(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.ApprenticeshipUpdate = null;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.OverlappingTrainingDateRequests = null;

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(0);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesForReview_When_ProviderSearch_On_EmployerOriginated_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = true;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
        {
            new ApprenticeshipUpdate
            {
                Originator = Originator.Employer,
                Status = ApprenticeshipUpdateStatus.Pending
            }
        };
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesForReview);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesPending_When_EmployerSearch_On_EmployerOriginated_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = false; 
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
        {
            new ApprenticeshipUpdate
            {
                Originator = Originator.Employer,
                Status = ApprenticeshipUpdateStatus.Pending
            }
        };
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesPending);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesPending_When_ProviderSearch_On_ProviderOriginated_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = true;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
        {
            new ApprenticeshipUpdate
            {
                Originator = Originator.Provider,
                Status = ApprenticeshipUpdateStatus.Pending
            }
        };
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesPending);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ChangesForReview_When_EmployerSearch_On_ProviderOriginated_PendingChanges(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = false;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
        {
            new ApprenticeshipUpdate
            {
                Originator = Originator.Provider,
                Status = ApprenticeshipUpdateStatus.Pending
            }
        };
        source.OverlappingTrainingDateRequests.Clear();

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ChangesForReview);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_No_Apprenticeship_Alerts_When_No_OverlappingTrainingDateRequests(
        Apprenticeship source,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.ApprenticeshipUpdate = null;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.OverlappingTrainingDateRequests = null;

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(0);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_Apprenticeship_Alerts_ConfirmDates_When_EmployerSearch_On_ConfirmDates(
        Apprenticeship source,
        OverlappingTrainingDateRequest overlappingTrainingDateRequest,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.IsProviderSearch = false;
        source.ApprovalRequests = new List<ApprovalRequest>();
        overlappingTrainingDateRequest.Status = OverlappingTrainingDateRequestStatus.Pending;
        source.ApprenticeshipUpdate = null;
        source.OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
        {
            overlappingTrainingDateRequest
        };

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(1);
        result.Alerts.First().Should().Be(Alerts.ConfirmDates);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Maps_All_Apprenticeship_Alerts_When_EmployerSearch(
        Apprenticeship source,
        OverlappingTrainingDateRequest overlappingTrainingDateRequest,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
        {
            overlappingTrainingDateRequest
        };
        source.IsProviderSearch = false;
        source.ApprovalRequests = new List<ApprovalRequest>();
        source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
        {
            new ApprenticeshipUpdate
            {
                Originator = Originator.Provider,
                Status = ApprenticeshipUpdateStatus.Pending
            }
        };

        var result = await mapper.Map(source);

        result.Alerts.Count().Should().Be(2);
        result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ConfirmDates, Alerts.ChangesForReview });
    }
        
    [Test, RecursiveMoqAutoData]
    public void Then_Throws_Exception_When_There_Is_No_PriceHistory_For_Apprenticeship(
        Apprenticeship source,
        OverlappingTrainingDateRequest overlappingTrainingDateRequest,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        
        source.PriceHistory = new List<PriceHistory>();
            
        var result =  () => mapper.Map(source);

        result.Should()
            .ThrowAsync<NullReferenceException>()
            .WithMessage($"There are no price history records for the apprenticeship Id: {source.Id}");
    }
    
    [Test, RecursiveMoqAutoData]
    public void Then_Throws_Exception_When_There_Is_Null_PriceHistory_For_Apprenticeship(
        Apprenticeship source,
        OverlappingTrainingDateRequest overlappingTrainingDateRequest,
        ApprenticeshipToApprenticeshipDetailsMapper mapper)
    {
        source.PriceHistory = null;
            
        var result =  () => mapper.Map(source);

        result.Should()
            .ThrowAsync<NullReferenceException>()
            .WithMessage($"There are no price history records for the apprenticeship Id: {source.Id}");
    }
}
