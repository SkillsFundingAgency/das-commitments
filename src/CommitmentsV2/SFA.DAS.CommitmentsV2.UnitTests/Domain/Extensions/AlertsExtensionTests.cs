using System.Collections.Generic;
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
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.DataLockStatus = new List<DataLockStatus>();
            source.PendingUpdateOriginator = null;

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_None_And_TriageStatus_Unknown_Then_No_Alerts(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.None;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            dataLockStatus.IsResolved = false;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> {Alerts.IlrDataMismatch});
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
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
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = false;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock07_And_TriageStatus_Change_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = false;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_ErrorCode_DLock03_And_TriageStatus_Restart_Then_Changes_Requested_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            dataLockStatus.IsResolved = false;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesRequested });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Provider_Then_Changes_Pending_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            apprenticeshipUpdate.Originator = Originator.Provider;
            apprenticeshipUpdate.Status = (byte) ApprenticeshipUpdateStatus.Pending;
            source.ApprenticeshipUpdate.Add(apprenticeshipUpdate);
            source.DataLockStatus = new List<DataLockStatus>(); 
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(new List<Alerts> { Alerts.ChangesPending });
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_PendingUpdateOriginator_Employer_Then_Changes_For_Review_Alert(
            Apprenticeship source,
            ApprenticeshipUpdate apprenticeshipUpdate,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {

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
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            source.PendingUpdateOriginator = null;
            source.ApprenticeshipUpdate = null;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            
            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task And_Has_Resolved_Alert_Then_Nothing_Is_Mapped(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            dataLockStatus.IsResolved = true;
            source.DataLockStatus = new List<DataLockStatus> { dataLockStatus };

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEmpty();
        }
    }
}