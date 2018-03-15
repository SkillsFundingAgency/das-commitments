using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class DataLockStatusGenerator : Generator
    {
        public async Task<IEnumerable<DbSetupDataLockStatus>> Generate(long firstNewApprenticeshipId, int numberOfNewApprenticeships)
        {
            var successDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                    TestDataVolume.SuccessDataLockStatusesToApprenticeshipsRatio);

            await SetUpFixture.LogProgress($"Generating {successDataLockStatusesToGenerate} success DataLockStatuses");

            var apprenticeshipIdsForDataLockStatuses = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.MaxDataLockStatusesPerApprenticeship);

            var testDataLockStatuses = Generate(apprenticeshipIdsForDataLockStatuses, successDataLockStatusesToGenerate, false);

            var errorDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                                                          TestDataVolume.ErrorDataLockStatusesToApprenticeshipsRatio);

            await SetUpFixture.LogProgress($"Generating {errorDataLockStatusesToGenerate} error DataLockStatuses");

            // needs to not include apprenticeshipId's that have success datalockstatuses
            // skip the apprenticeship ids we used for the success DataLockStatuses
            var randomlyGroupedErrorApprenticeshipIds = apprenticeshipIdsForDataLockStatuses.Skip(testDataLockStatuses.Count());
            // the first id may have been in a group where the id was already used for success, so skip that
            var firstIdInRemaining = randomlyGroupedErrorApprenticeshipIds.First();
            randomlyGroupedErrorApprenticeshipIds.SkipWhile(i => i == firstIdInRemaining);

            testDataLockStatuses = testDataLockStatuses.Concat(Generate(randomlyGroupedErrorApprenticeshipIds, errorDataLockStatusesToGenerate, true));

            // shuffle the DataLockStatuses so that all the error rows aren't grouped at the end
            // we'll do it this way here (if it wasn't test code, perhaps we'd do it differently)
            // see https://stackoverflow.com/questions/6569422/how-can-i-randomly-ordering-an-ienumerable

            return testDataLockStatuses.OrderBy(s => Random.Next());
        }

        private IEnumerable<DbSetupDataLockStatus> Generate(IEnumerable<long> randomlyOrderedApprenticeshipIdGroups, int dataLockStatusesToGenerate, bool setError = false)
        {
            var dataLockStatuses = new Fixture().CreateMany<DbSetupDataLockStatus>(dataLockStatusesToGenerate).ToList();

            //todo: don't use autofixture for these, just generate the whole lot by hand?
            return dataLockStatuses.Zip(randomlyOrderedApprenticeshipIdGroups, (dataLockStatus, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope. could create new
                dataLockStatus.ApprenticeshipId = apprenticeshipId;
                dataLockStatus.Status = GenerateStatus(setError);
                dataLockStatus.ErrorCode = GenerateDataLockError(setError);
                dataLockStatus.TriageStatus = GenerateTriageStatus(dataLockStatus.ErrorCode);
                dataLockStatus.IsResolved = GenerateIsResolved(dataLockStatus.TriageStatus);
                dataLockStatus.EventStatus = TestDataVolume.DataLockStatusEventStatusProbability.NextRandom();
                // all are currently unexpired, but we might get some next academic year
                //dataLockStatus.IsExpired = false;

                return dataLockStatus;
            });
        }

        private Status GenerateStatus(bool error)
        {
            //todo: what about unknown?
            return error ? Status.Fail : Status.Pass;
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

        private DataLockErrorCode GenerateDataLockError(bool error)
        {
            if (!error)
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
