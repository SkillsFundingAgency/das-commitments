using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    //todo: generating 1/4 million objects with autofixture is slow. speed it up!
    public class TestData
    {
        private readonly string _databaseConnectionString;
        public CommitmentsDatabase CommitmentsDatabase { get; }
        private static readonly Random Random = new Random();

        public TestData()
        {
            var config = Infrastructure.Configuration.Configuration.Get();
            _databaseConnectionString = config.DatabaseConnectionString;

            CommitmentsDatabase = new CommitmentsDatabase(_databaseConnectionString);
        }

        public async Task<TestIds> Initialise()
        {
            const string schemaVersionColumnName = "IntTest_SchemaVersion";

            //todo: handle case when first run against old database (without schema version) - if vsts deploy deploys database, should be ok

            var databaseManagement = new DatabaseManagement(_databaseConnectionString);

            if (!await databaseManagement.Exists())
            {
                databaseManagement.Publish();
            }
            else // database already exists
            {
                var existingDatabaseSchemaVersion = await CommitmentsDatabase.GetJobProgress<int?>(schemaVersionColumnName);
                if (existingDatabaseSchemaVersion != CommitmentsDatabase.SchemaVersion)
                {
                    // if we can get the deploy options working, we won't need to do this...
                    databaseManagement.KillAzure();
                    databaseManagement.Publish();
                    await CommitmentsDatabase.SetJobProgress(schemaVersionColumnName, CommitmentsDatabase.SchemaVersion);
                }
                else // correct schema version
                {
                    if (await CommitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName) >= TestDataVolume.MinNumberOfApprenticeships)
                    {
                        return await TestIds.Fetch(_databaseConnectionString);
                    }
                }
            }

            return await PopulateDatabaseWithTestDataAndStoreTestApprenticeshipIds();
        }

        public async Task<TestIds> PopulateDatabaseWithTestDataAndStoreTestApprenticeshipIds()
        {
            //todo: if this fails for some reason, do we clean up? or put whole lot in transaction, or leave and try and recover next run?
            var testIds = await PopulateDatabaseWithTestData();
            await testIds.Store(_databaseConnectionString);
            return testIds;
        }

        public async Task<TestIds> PopulateDatabaseWithTestData()
        {
            //do we want to support 'adding' to existing data, or make it son always populate from fresh db?
            //there are currently all sorts of edge cases for expansion
            //but if when deal with million+ rows and want to add another million might be handy to add more
            // could assert corner cases rather than trying to handle them?
            // if adding, would probably be better to leave current test ids alone and just generate for volume
            var firstNewApprenticeshipId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipTableName);
            var firstNewCohortId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.CommitmentTableName);

            var apprenticeshipsInTable =
                await CommitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName);

            var apprenticeshipsToGenerate = TestDataVolume.MinNumberOfApprenticeships - apprenticeshipsInTable;

            var testIds = new TestIds();

            if (apprenticeshipsToGenerate > 0)
            {
                (var testApprenticeships, long lastCohortId) = GenerateApprenticeships(testIds, apprenticeshipsToGenerate,
                    firstNewApprenticeshipId, firstNewCohortId);
                await CommitmentsDatabase.InsertApprenticeships(testApprenticeships);

                // generate the commitments that the new apprenticeships reference
                int commitmentsToGenerate = (int) (1 + lastCohortId - firstNewCohortId);

                await SetUpFixture.LogProgress("Generating Commitments");

                var testCommitments = GenerateCommitments(commitmentsToGenerate);
                await CommitmentsDatabase.InsertCommitments(testCommitments);
            }

            //todo: consistent param ordering
            await PopulateApprenticeshipUpdates(apprenticeshipsToGenerate, firstNewApprenticeshipId);

            await PopulateDataLockStatuses(firstNewApprenticeshipId, apprenticeshipsToGenerate);

            return testIds;
        }

        private async Task PopulateApprenticeshipUpdates(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            //todo: methods for these
            // generate apprenticeship updates
            int apprenticeshipUpdatesToGenerate = (int)(apprenticeshipsGenerated * TestDataVolume.ApprenticeshipUpdatesToApprenticeshipsRatio);
            //var firstNewApprenticeshipUpdateId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipUpdateTableName);

            var testApprenticeshipUpdates = GenerateApprenticeshipUpdates(firstNewApprenticeshipId, apprenticeshipsGenerated, apprenticeshipUpdatesToGenerate);
            await CommitmentsDatabase.InsertApprenticeshipUpdates(testApprenticeshipUpdates);
        }

        private async Task PopulateDataLockStatuses(long firstNewApprenticeshipId, int numberOfNewApprenticeships)
        {
            // generate success DataLockStatuses
            var successDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                    TestDataVolume.SuccessDataLockStatusesToApprenticeshipsRatio);

            var apprenticeshipIdsForDataLockStatuses = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.MaxDataLockStatusesPerApprenticeship);

            var testDataLockStatuses = GenerateDataLockStatuses(apprenticeshipIdsForDataLockStatuses, successDataLockStatusesToGenerate, false);

            var errorDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                                                          TestDataVolume.ErrorDataLockStatusesToApprenticeshipsRatio);

            // needs to not include apprenticeshipId's that have success datalockstatuses
            // skip the apprenticeship ids we used for the success DataLockStatuses
            var randomlyGroupedErrorApprenticeshipIds = apprenticeshipIdsForDataLockStatuses.Skip(testDataLockStatuses.Count);
            // the first id may have been in a group where the id was already used for success, so skip that
            var firstIdInRemaining = randomlyGroupedErrorApprenticeshipIds.First();
            randomlyGroupedErrorApprenticeshipIds.SkipWhile(i => i == firstIdInRemaining);

            testDataLockStatuses.AddRange(GenerateDataLockStatuses(randomlyGroupedErrorApprenticeshipIds, errorDataLockStatusesToGenerate, true));

            // shuffle the DataLockStatuses so that all the error rows aren't grouped at the end
            // we'll do it this way here (if it wasn't test code, perhaps we'd do it differently)
            // see https://stackoverflow.com/questions/6569422/how-can-i-randomly-ordering-an-ienumerable
            testDataLockStatuses = testDataLockStatuses.OrderBy(s => Random.Next()).ToList();

            await CommitmentsDatabase.InsertDataLockStatuses(testDataLockStatuses);
        }

        public (List<DbSetupApprenticeship>, long) GenerateApprenticeships(TestIds testIds, int apprenticeshipsToGenerate, long initialId = 1, long firstCohortId = 1, int maxCohortSize = TestDataVolume.MaxNumberOfApprenticeshipsInCohort)
        {
            var apprenticeships = new Fixture().CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate).ToList();

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max
            //todo: get's id that isn't generated if rows are already generated!
            testIds[TestIds.MaxCohortSize] = initialId;

            int apprenticeshipsLeftInCohort = maxCohortSize;

            foreach (var apprenticeship in apprenticeships)
            {
                if (--apprenticeshipsLeftInCohort < 0)
                {
                    apprenticeshipsLeftInCohort = Random.Next(1, maxCohortSize+1);
                    ++firstCohortId;
                }

                apprenticeship.CommitmentId = firstCohortId;
            }

            return (apprenticeships, firstCohortId);
        }

        public List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdates(long firstNewApprenticeshipId, int numberOfNewApprenticeships, int apprenticeshipUpdatesToGenerate)
        {
            var apprenticeshipIdsForUpdates = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship);

            var apprenticeshipUpdates = new Fixture().CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();

            return apprenticeshipUpdates.Zip(apprenticeshipIdsForUpdates, (update, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                update.ApprenticeshipId = apprenticeshipId;
                return update;
            }).ToList();
        }

        public IEnumerable<long> RandomIdGroups(long firstId, int countOfIds, int maxIdsPerGroup)
        {
            // limit length? does it matter if lazily enumerated and don't read past required?
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next(int.MaxValue));

            return newApprenticeshipIdsShuffled.SelectMany(id => Enumerable.Repeat((long)id, Random.Next(1, maxIdsPerGroup + 1)));
        }

        public static List<DbSetupCommitment> GenerateCommitments(int commitmentsToGenerate)
        {
            var fixture = new Fixture();
            var commitments = fixture.CreateMany<DbSetupCommitment>(commitmentsToGenerate).ToList();
            foreach (var commitment in commitments)
            {
                // we'll probably have to do better than this at some point, but this might be enough for the initial tests
                // if we do something a bit closer to real-world, we'll have to add probably 2
                // extra columns to IntegrationTestIds, EmployerId & ProviderId (or possibly 1 column as a (c++ style) union)
                commitment.EmployerAccountId = commitment.Id;
            }
            return commitments;
        }

        public static List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdate(int apprenticeshipUpdatesToGenerate, long initialId = 1)
        {
            return new Fixture().CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();
        }

        public List<DbSetupDataLockStatus> GenerateDataLockStatuses(IEnumerable<long> randomlyOrderedApprenticeshipIdGroups, int dataLockStatusesToGenerate, bool setError = false)
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
            }).ToList();
        }

        private Status GenerateStatus(bool error)
        {
            //todo: what about unknown?
            return error ? Status.Fail: Status.Pass;
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

            var numberOfFlags = Random.Next(1, 3+1);
            int errorCode = 0;
            while (numberOfFlags-- > 0)
            {
                errorCode |= 1 << Random.Next(9+1);
            }
            return (DataLockErrorCode)errorCode;
        }

        public static long GetRandomApprenticeshipId(HashSet<long> exclude = null)
        {
            if (exclude == null)
                return Random.Next(1, TestDataVolume.MinNumberOfApprenticeships + 1);

            long apprenticeshipId;
            while (exclude.Contains(apprenticeshipId = Random.Next(1, TestDataVolume.MinNumberOfApprenticeships + 1)))
            {
            }

            return apprenticeshipId;
        }
    }
}
