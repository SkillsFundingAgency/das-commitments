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
            
            var result = mapper.Map(source);

            result.Should().BeEquivalentTo(new List<string> {"Changes pending"});
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
    }
}