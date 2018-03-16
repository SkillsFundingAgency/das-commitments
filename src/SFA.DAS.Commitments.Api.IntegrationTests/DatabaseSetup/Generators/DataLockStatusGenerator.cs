using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    /// <remarks>
    /// We generate more realistic data than the slowdown test requires, but the idea is that when we add additional integration tests, they can use the generated data.
    /// If your new test requires more realistic data, then update the generation here to the benefit of all tests.
    /// </remarks>
    public class DataLockStatusGenerator : Generator
    {
        private static readonly DateTime SystemStartDate = new DateTime(2016, 8, 1);
        private static readonly int MonthsSinceSystemStartDate = (int)((DateTime.Now - SystemStartDate).TotalDays / (365.25 / 12));
        private static readonly int SecondsSinceSystemStartDate = (int)(DateTime.Now - SystemStartDate).TotalSeconds;
        private static readonly string[] ExampleFrameworkCourseCodes = { "583-3-4", "548-3-1", "418-2-1" };

        public async Task<IEnumerable<DbSetupDataLockStatus>> Generate(int numberOfNewApprenticeships, long firstNewApprenticeshipId, long firstNewDataLockEventId)
        {
            // generate id groups first with probability distribution, then generate failed from the actual number generated
            // we lose the absolute ratio of statuses to apprenticeships, but probabilitydistribution will act as the ratio in effect
            var randomlyOrderedApprenticeshipIdGroups = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.DataLockStatusesPerApprenticeshipProbability);

            var totalDataLockStatusesToGenerate = randomlyOrderedApprenticeshipIdGroups.Count();

            await SetUpFixture.LogProgress($"Generating {totalDataLockStatusesToGenerate} DataLockStatuses");

            var errorDataLockStatusesToGenerate = (int)(totalDataLockStatusesToGenerate * TestDataVolume.ErrorDataLockStatusProbability);

            // as we have a potentially large range and require only a small random subset, we do it this way...
            //todo: methodize
            int randomRangeTop = (int)(firstNewDataLockEventId + totalDataLockStatusesToGenerate);
            var failedDataLockStatusIds = new HashSet<long>();
            while (failedDataLockStatusIds.Count < errorDataLockStatusesToGenerate)
            {
                failedDataLockStatusIds.Add(Random.Next((int)firstNewDataLockEventId, randomRangeTop));
            }

            var dataLockEventIds = Enumerable.Range((int)firstNewDataLockEventId, totalDataLockStatusesToGenerate);

            return dataLockEventIds.Zip(randomlyOrderedApprenticeshipIdGroups,
                (dataLockEventId, apprenticeshipId) =>
                    GenerateDbSetupDataLockStatus(dataLockEventId, apprenticeshipId, failedDataLockStatusIds));
        }

        private DbSetupDataLockStatus GenerateDbSetupDataLockStatus(long dataLockEventId, long apprenticeshipId, HashSet<long> failedDataLockStatusIds)
        {
            var startDate = GenerateStartDate();

            //todo: what about unknown?
            Status status = failedDataLockStatusIds.Contains(dataLockEventId) ? Status.Fail : Status.Pass;

            var dataLockStatus = new DbSetupDataLockStatus
            {
                DataLockEventId = dataLockEventId,
                ApprenticeshipId = apprenticeshipId,
                Status = status,
                ErrorCode = GenerateDataLockError(status),
                EventStatus = TestDataVolume.DataLockStatusEventStatusProbability.NextRandom(),
                IlrActualStartDate = startDate, //GenerateStartDate(),
                IlrEffectiveFromDate = startDate,
                IlrPriceEffectiveToDate = GenerateIlrPriceEffectiveToDate(startDate),
                IlrTotalCost = GenerateTotalCost(),
                DataLockEventDatetime = GenerateDataLockEventDatetime(),
                IlrTrainingType = GenerateIlrTrainingType()
                // all are currently unexpired, but we might get some next academic year
                // IsExpired = false;
            };
            dataLockStatus.TriageStatus = GenerateTriageStatus(dataLockStatus.ErrorCode);
            dataLockStatus.IsResolved = GenerateIsResolved(dataLockStatus.TriageStatus);
            dataLockStatus.IlrTrainingCourseCode = GenerateIlrTrainingCourseCode(dataLockStatus.IlrTrainingType);
            //                dataLockStatus.PriceEpisodeIdentifier = $"{dataLockStatus.IlrTrainingCourseCode}-{dataLockStatus.IlrActualStartDate:d}"; //todo: also simulate 01/08/2018
            // until we sort out breaking the unique index
            dataLockStatus.PriceEpisodeIdentifier = $"{Guid.NewGuid().ToString().Substring(0,25)}";

            return dataLockStatus;
        }

        private TrainingType GenerateIlrTrainingType()
        {
            return (TrainingType) Random.Next(2);
        }

        private string GenerateIlrTrainingCourseCode(TrainingType trainingType)
        {
            //new TrainingCourse { FworkCode = "583", ProgType = "3", PwayCode = "4" },
            //new TrainingCourse { FworkCode = "548", ProgType = "3", PwayCode = "1" },
            //new TrainingCourse { FworkCode = "418", ProgType = "2", PwayCode = "1" },
            //new TrainingCourse { ProgType = "25", StandardCode = "6" },

            //return trainingType == TrainingType.Standard
            //    ? $"{dataLockEvent.IlrStandardCode}" :
            //    $"{dataLockEvent.IlrFrameworkCode}-{dataLockEvent.IlrProgrammeType}-{dataLockEvent.IlrPathwayCode}";

            return trainingType == TrainingType.Standard
                ? "6"
                : ExampleFrameworkCourseCodes[Random.Next(ExampleFrameworkCourseCodes.Length)];
        }

        private DateTime GenerateDataLockEventDatetime()
        {
            return SystemStartDate + new TimeSpan(0, 0, Random.Next(SecondsSinceSystemStartDate));
        }

        private DateTime GenerateStartDate()
        {
            return SystemStartDate.AddMonths(Random.Next(0, MonthsSinceSystemStartDate+6));
        }

        private DateTime? GenerateIlrPriceEffectiveToDate(DateTime? ilrEffectiveFromDate)
        {
            return ilrEffectiveFromDate?.AddMonths(3 + Random.Next(9));
        }

        private decimal? GenerateTotalCost()
        {
            return 200+(Random.Next(300)*10);
        }

        //private Status GenerateStatus(GenerateType generateType)
        //{
        //    //todo: what about unknown?
        //    return generateType == GenerateType.Success ? Status.Pass : Status.Fail;
        //}

        private TriageStatus GenerateTriageStatus(DataLockErrorCode errorCode)
        {
            if (errorCode == DataLockErrorCode.None)
                return TriageStatus.Unknown;

            // if errorcode is one of the 4 change codes
            if ((errorCode &
                 (DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04 | DataLockErrorCode.Dlock05 | DataLockErrorCode.Dlock06)) != 0)
            {
                return Random.Next(2) == 0 ? TriageStatus.Restart : TriageStatus.Unknown;
            }

            if ((errorCode & (DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock09)) != 0)
            {
                return Random.Next(2) == 0 ? TriageStatus.Change : TriageStatus.Unknown;
            }

            // FixInIlr is not currently used
            return TriageStatus.Unknown;
        }

        private bool GenerateIsResolved(TriageStatus triageStatus)
        {
            return triageStatus == TriageStatus.Unknown ? false : Random.Next(2) == 0;
        }

        private DataLockErrorCode GenerateDataLockError(Status status)
        {
            if (status != Status.Fail)
                return DataLockErrorCode.None;

            var numberOfFlags = Random.Next(1, 3 + 1);
            int errorCode = 0;
            while (numberOfFlags-- > 0)
            {
                errorCode |= 1 << Random.Next(9 + 1);
            }
            return (DataLockErrorCode)errorCode;
        }
    }
}
