using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
using Apprenticeship = SFA.DAS.CommitmentsV2.Models.Apprenticeship;
using ApprenticeshipUpdate = SFA.DAS.CommitmentsV2.Models.ApprenticeshipUpdate;
using ApprenticeshipUpdateStatus = SFA.DAS.CommitmentsV2.Types.ApprenticeshipUpdateStatus;
using Originator = SFA.DAS.CommitmentsV2.Types.Originator;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Extensions
{
    public class AlertsExtensionTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task And_No_DataLocks_Then_No_Alerts(
            Apprenticeship source,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.DataLockStatus = new List<DataLockStatus>();
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_None_And_TriageStatus_Unknown_Then_No_Alerts(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.None;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.IlrDataMismatch });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.IsProviderSearch = true;
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.IlrDataMismatch });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Change_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Change_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.Status = Status.Fail;
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Restart_Then_Changes_Requested_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesRequested });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Provider_And_IsProviderSearch_Then_Changes_Pending_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            apprenticeshipUpdate.Originator = Originator.Provider;
            apprenticeshipUpdate.Status = (byte)ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(apprenticeshipUpdate);
            source.DataLockStatus = new List<DataLockStatus>();
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.IsProviderSearch = true;
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Employer_And_IsProviderSearch_Then_Changes_For_Review_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            apprenticeshipUpdate.Originator = Originator.Employer;
            apprenticeshipUpdate.Status = (byte)ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(new ApprenticeshipUpdate
            {
                Originator = (byte)Originator.Employer
            });
            source.DataLockStatus = new List<DataLockStatus>();
            source.IsProviderSearch = true;
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesForReview });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Provider_And_IsNotProviderSearch_Then_Changes_for_Review_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            apprenticeshipUpdate.Originator = Originator.Provider;
            apprenticeshipUpdate.Status = (byte)ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(apprenticeshipUpdate);
            source.DataLockStatus = new List<DataLockStatus>();
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.IsProviderSearch = false;
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesForReview });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Employer_And_IsNotProviderSearch_Then_Changes_Pending_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            apprenticeshipUpdate.Originator = Originator.Employer;
            apprenticeshipUpdate.Status = (byte)ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(new ApprenticeshipUpdate
            {
                Originator = (byte)Originator.Employer
            });
            source.DataLockStatus = new List<DataLockStatus>();
            source.IsProviderSearch = false;
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Null_Then_No_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.ApprenticeshipUpdate = null;
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_Resolved_Alert_Then_Nothing_Is_Mapped(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = true;
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Employer_Has_Unresolved_Errors_That_Have_Known_Triage_Status(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            //Arrange
            dataLockStatus.Status = Status.Fail;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            source.IsProviderSearch = false;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            //Act
            var result = await mapper.Map(source);

            //Assert
            result.Alerts.Should().NotBeNullOrEmpty();
            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesRequested });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Employer_Has_Unresolved_Errors_That_Have_Known_Triage_Status_And_Has_Course_DataLock_Changes_Requested_Only_One_Changes_Requested_Added(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            DataLockStatus dataLockStatus2,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            //Arrange
            dataLockStatus.Status = Status.Fail;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = false;
            dataLockStatus2.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus2.TriageStatus = TriageStatus.Restart;
            dataLockStatus2.IsResolved = false;
            dataLockStatus2.IsExpired = false;
            source.IsProviderSearch = false;
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus, dataLockStatus2 };
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            //Act
            var result = await mapper.Map(source);

            //Assert
            result.Alerts.Should().NotBeNullOrEmpty();
            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesRequested });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Provider_Has_Unresolved_Errors_That_Have_Known_Triage_Status(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            //Arrange
            dataLockStatus.Status = Status.Fail;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            dataLockStatus.IsResolved = false;
            source.IsProviderSearch = true;
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            //Act
            var result = await mapper.Map(source);

            //Assert
            result.Alerts.Should().BeNullOrEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Unknown_And_DataLock_HasExpired_Then_No_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.IsProviderSearch = true;
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = true;

            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeNullOrEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Change_And_DataLock_HasExpired_Then_No_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = false;
            dataLockStatus.IsExpired = true;

            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.OverlappingTrainingDateRequests = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeNullOrEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingOverlappingTrainingDateRequests_Employer_And_IsNotProviderSearch_Then_ConfirmDates_Alert(
          Apprenticeship source,
          ApprenticeshipUpdate apprenticeshipUpdate,
          OverlappingTrainingDateRequest overlappingTrainingDateRequest,
          ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.DataLockStatus = new List<DataLockStatus>();
            source.IsProviderSearch = false;

            overlappingTrainingDateRequest.Status = OverlappingTrainingDateRequestStatus.Pending;

            source.OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
            {
                overlappingTrainingDateRequest
            };

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ConfirmDates });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_NoPendingOverlappingTrainingDateRequests_Is_Employer_And_IsNotProviderSearch_Then_No_ConfirmDates_Alert(
          Apprenticeship source,
          ApprenticeshipUpdate apprenticeshipUpdate,
          OverlappingTrainingDateRequest overlappingTrainingDateRequest,
          ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.DataLockStatus = new List<DataLockStatus>();
            source.IsProviderSearch = false;

            overlappingTrainingDateRequest.Status = OverlappingTrainingDateRequestStatus.Resolved;

            source.OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
            {
                overlappingTrainingDateRequest
            };

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingOverlappingTrainingDateRequests_Is_Employer_And_IsProviderSearch_Then_No_ConfirmDates_Alert(
         Apprenticeship source,
         ApprenticeshipUpdate apprenticeshipUpdate,
         OverlappingTrainingDateRequest overlappingTrainingDateRequest,
         ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            source.DataLockStatus = new List<DataLockStatus>();

            overlappingTrainingDateRequest.Status = OverlappingTrainingDateRequestStatus.Pending;
            source.OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
            {
                overlappingTrainingDateRequest
            };

            source.IsProviderSearch = true;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }
    }
}