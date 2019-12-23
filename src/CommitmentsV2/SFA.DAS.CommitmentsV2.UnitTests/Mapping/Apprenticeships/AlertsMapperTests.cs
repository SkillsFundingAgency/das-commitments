using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships
{
    public class AlertsMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public void And_No_DataLocks_Then_No_Alerts(
            Apprenticeship source,
            AlertsMapper mapper)
        {
            source.DataLockStatus = new List<DataLockStatus>();
            source.PendingUpdateOriginator = null;

            var result = mapper.Map(source);

            result.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_None_And_TriageStatus_Unknown_Then_No_Alerts(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.None;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_DLock03_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"ILR data mismatch"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_DLock07_And_TriageStatus_Unknown_Then_ILR_Data_mismatch_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"ILR data mismatch"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_DLock03_And_TriageStatus_Change_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes pending"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_DLock07_And_TriageStatus_Change_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock07;
            dataLockStatus.TriageStatus = TriageStatus.Change;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes pending"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_ErrorCode_DLock03_And_TriageStatus_Restart_Then_Changes_Requested_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            dataLockStatus.ErrorCode = DataLockErrorCode.Dlock03;
            dataLockStatus.TriageStatus = TriageStatus.Restart;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            source.PendingUpdateOriginator = null;
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes requested"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_PendingUpdateOriginator_Provider_Then_Changes_Pending_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            source.PendingUpdateOriginator = Originator.Provider;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes pending"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_PendingUpdateOriginator_Employer_Then_Changes_For_Review_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            source.PendingUpdateOriginator = Originator.Employer;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes for review"});
        }

        [Test, RecursiveMoqAutoData]
        public void And_Has_PendingUpdateOriginator_Null_Then_No_Alert(
            Apprenticeship source,
            DataLockStatus dataLockStatus,
            AlertsMapper mapper)
        {
            source.PendingUpdateOriginator = null;
            source.DataLockStatus = new List<DataLockStatus>{dataLockStatus};
            
            var result = mapper.Map(source);

            result.Should().BeEmpty();
        }

        // price will have change 
        // course could be 0
    }

    public class AlertsMapper
    {
        public IEnumerable<string> Map(Apprenticeship source)
        {
            var result = new List<string>();

            if (HasCourseDataLock(source) ||
                HasPriceDataLock(source))
            {
                result.Add("ILR data mismatch");
            }

            if (HasCourseDataLockPendingChanges(source) ||
                HasPriceDataLockPendingChanges(source))
            {
                result.Add("Changes pending");
            }

            if (HasCourseDataLockChangesRequested(source))
            {
                result.Add("Changes requested");
            }

            if (!source.PendingUpdateOriginator.HasValue) 
                return result;

            result.Add(source.PendingUpdateOriginator == Originator.Provider
                ? "Changes pending"
                : "Changes for review");

            return result;
        }

        private bool HasCourseDataLock(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x => 
                x.WithCourseError() && 
                x.TriageStatus == TriageStatus.Unknown);
        }

        private bool HasPriceDataLock(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x => 
                x.IsPriceOnly() && 
                x.TriageStatus == TriageStatus.Unknown);
        }

        private bool HasCourseDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Change);
        }

        private bool HasPriceDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.IsPriceOnly() && 
                x.TriageStatus == TriageStatus.Change);
        }

        private bool HasCourseDataLockChangesRequested(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Restart);
        }
    }
}