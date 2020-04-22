﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
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
            source.PriceHistory = new List<PriceHistory>{priceHistory};
            source.PendingUpdateOriginator = null;

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
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
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
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> {Alerts.IlrDataMismatch});
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            dataLockStatus.IsResolved = false;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
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
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
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
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
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
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesRequested });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Provider_Then_Changes_Pending_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            apprenticeshipUpdate.Originator = Originator.Provider;
            apprenticeshipUpdate.Status = (byte) ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(apprenticeshipUpdate);
            source.DataLockStatus = new List<DataLockStatus>();
            source.PriceHistory = new List<PriceHistory> { priceHistory };

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Employer_Then_Changes_For_Review_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory> { priceHistory };
            apprenticeshipUpdate.Originator = Originator.Employer;
            apprenticeshipUpdate.Status = (byte) ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(new ApprenticeshipUpdate
            {
                Originator = (byte)Originator.Employer
            });
            source.DataLockStatus = new List<DataLockStatus>();
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesForReview });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Null_Then_No_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            PriceHistory priceHistory,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PendingUpdateOriginator = null;
            source.ApprenticeshipUpdate = null;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PriceHistory = new List<PriceHistory> { priceHistory };

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
            source.IsProviderSearch = false;
            source.PriceHistory = new List<PriceHistory>{ priceHistory };
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };

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
            dataLockStatus2.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus2.TriageStatus = TriageStatus.Restart;
            dataLockStatus2.IsResolved = false;
            source.IsProviderSearch = false;
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus, dataLockStatus2 };
            source.PriceHistory = new List<PriceHistory> { priceHistory };

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

            //Act
            var result = await mapper.Map(source);

            //Assert
            result.Alerts.Should().BeNullOrEmpty();
        }
    }
}