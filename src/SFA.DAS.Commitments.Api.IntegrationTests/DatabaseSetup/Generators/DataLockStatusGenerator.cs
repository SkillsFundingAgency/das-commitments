using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
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
            var randomlyOrderedApprenticeshipIdGroups = RandomIdGroups(firstNewApprenticeshipId,
                numberOfNewApprenticeships, TestDataVolume.DataLockStatusesPerApprenticeshipProbability,
                (id, count) => GenerateStartDates(count).Select(date => new IdAndDate {Id = id, Date = date}));

            var totalDataLockStatusesToGenerate = randomlyOrderedApprenticeshipIdGroups.Length;

            await TestLog.Progress($"Generating {totalDataLockStatusesToGenerate} DataLockStatuses");

            var errorDataLockStatusesToGenerate = (int)(totalDataLockStatusesToGenerate * TestDataVolume.ErrorDataLockStatusProbability);

            var failedDataLockStatusIds = FailedDataLockStatusIds(firstNewDataLockEventId, totalDataLockStatusesToGenerate, errorDataLockStatusesToGenerate);

            var dataLockEventIds = Enumerable.Range((int)firstNewDataLockEventId, totalDataLockStatusesToGenerate);

            return dataLockEventIds.Zip(randomlyOrderedApprenticeshipIdGroups,
                (dataLockEventId, apprenticeshipIdAndStartDate) =>
                    GenerateDbSetupDataLockStatus(dataLockEventId, apprenticeshipIdAndStartDate, failedDataLockStatusIds));
        }

        private HashSet<long> FailedDataLockStatusIds(long firstNewDataLockEventId, int totalDataLockStatuses,
            int errorDataLockStatusesToGenerate)
        {
            // as we have a potentially large range and require only a small random subset, we do it this way...

            int randomRangeTop = (int) (firstNewDataLockEventId + totalDataLockStatuses);
            var failedDataLockStatusIds = new HashSet<long>();
            while (failedDataLockStatusIds.Count < errorDataLockStatusesToGenerate)
            {
                failedDataLockStatusIds.Add(Random.Next((int) firstNewDataLockEventId, randomRangeTop));
            }

            return failedDataLockStatusIds;
        }

        private class IdAndDate
        {
            public long Id;
            public DateTime Date;
        }

        private DbSetupDataLockStatus GenerateDbSetupDataLockStatus(long dataLockEventId, IdAndDate apprenticeshipIdAndStartDate, HashSet<long> failedDataLockStatusIds)
        {
            //todo: what about unknown?
            var status = failedDataLockStatusIds.Contains(dataLockEventId) ? Status.Fail : Status.Pass;

            var dataLockStatus = new DbSetupDataLockStatus
            {
                DataLockEventId = dataLockEventId,
                ApprenticeshipId = apprenticeshipIdAndStartDate.Id,
                Status = status,
                ErrorCode = GenerateDataLockError(status),
                EventStatus = TestDataVolume.DataLockStatusEventStatusProbability.NextRandom(),
                IlrActualStartDate = apprenticeshipIdAndStartDate.Date,
                IlrEffectiveFromDate = apprenticeshipIdAndStartDate.Date,
                IlrPriceEffectiveToDate = GenerateIlrPriceEffectiveToDate(apprenticeshipIdAndStartDate.Date),
                IlrTotalCost = GenerateTotalCost(),
                DataLockEventDatetime = GenerateDataLockEventDatetime(),
                IlrTrainingType = GenerateIlrTrainingType()
                // all are currently unexpired, but we might get some next academic year
                // IsExpired = false;
            };
            dataLockStatus.TriageStatus = GenerateTriageStatus(dataLockStatus.ErrorCode);
            dataLockStatus.IsResolved = GenerateIsResolved(dataLockStatus.TriageStatus);
            dataLockStatus.IlrTrainingCourseCode = GenerateIlrTrainingCourseCode(dataLockStatus.IlrTrainingType);
            dataLockStatus.PriceEpisodeIdentifier = $"{dataLockStatus.IlrTrainingCourseCode}-{apprenticeshipIdAndStartDate.Date:d}"; //todo: also simulate 01/08/2018

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

        private IEnumerable<DateTime> GenerateStartDates(int count)
        {
            // case 0 and 1 aren't necessary, they're just optimisations
            switch (count)
            {
                case 0:
                    return Enumerable.Empty<DateTime>();
                case 1:
                    return Enumerable.Repeat(GenerateStartDate(), 1);
                default:
                    return Enumerable.Range(0, MonthsSinceSystemStartDate + 6).OrderBy(m => Random.Next()).Take(count)
                        .Select(m => SystemStartDate.AddMonths(m)).OrderBy(d => d);
            }
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
            return Random.Next(300)*10+200;
        }

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
