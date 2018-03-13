using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    //todo: generating 1/4 million objects with autofixture is slow. speed it up!
    public class TestData
    {
        //private readonly CommitmentsApiConfiguration _config;
        private readonly string _databaseConnectionString;
        public CommitmentsDatabase CommitmentsDatabase { get; }
        //private readonly TestIds _ids;
        //do we want to bother wrapping this dictionary? would also contain names
        //private readonly Dictionary<string, long> _ids = new Dictionary<string, long>();

        private static readonly Random Random = new Random();

        public TestData()
        {
            var config = Infrastructure.Configuration.Configuration.Get();
            _databaseConnectionString = config.DatabaseConnectionString;

            CommitmentsDatabase = new CommitmentsDatabase(_databaseConnectionString);
        }

        public async Task<TestIds> Initialise()
        {
            //logger null gonna cause probs?
            //var jobProgressRepository = new JobProgressRepository(_config.DatabaseConnectionString, null);
            //jobProgressRepository.

            // todo: check if db there and publish if necessary (to sql azure would be best to more closely match live)

            const string schemaVersionColumnName = "IntTest_SchemaVersion";

            //todo: handle case when first run against old database (without schema version) - if vsts deploy deploys database, should be ok

            var databaseManagement = new DatabaseManagement(_databaseConnectionString);

            if (!await databaseManagement.Exists())
            {
                databaseManagement.Publish();
            }
            else
            {
                var existingDatabaseSchemaVersion = await CommitmentsDatabase.GetJobProgress<int?>(schemaVersionColumnName);
                if (existingDatabaseSchemaVersion != CommitmentsDatabase.SchemaVersion)
                {
                    //todo:await CommitmentsDatabase.ClearData();
                    //required with recreate?

                    // if we can get the deploy options working, we won't need to do this...
                    databaseManagement.KillAzure();

                    databaseManagement.Publish();
                    //var testIdNames = await PopulateDatabaseWithTestDataAndStoreTestApprenticeshipIds();
                    await CommitmentsDatabase.SetJobProgress(schemaVersionColumnName, CommitmentsDatabase.SchemaVersion);
                    //return testIdNames;
                }
            }

            // > 0, or less than MinNumberOfApprenticeships? what if more data?
            if (await CommitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName) >= 
                TestDataVolume.MinNumberOfApprenticeships)
            {
                return await TestIds.Fetch(_databaseConnectionString);
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
            //todo: this code generates success dls and then error dls, which might not be good enough
            // we might have to intersperse the error statuses throughout the table - we could shuffle the result before writing it to the database
            // check what clustered index is used

            //var firstNewDataLockStatusUpdateId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.DataLockStatusTableName);

            // generate success DataLockStatuses
            var successDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                    TestDataVolume.SuccessDataLockStatusesToApprenticeshipsRatio);

            var apprenticeshipIdsForDataLockStatuses = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.MaxDataLockStatusesPerApprenticeship);

            var testDataLockStatuses = GenerateDataLockStatuses(apprenticeshipIdsForDataLockStatuses, successDataLockStatusesToGenerate, false);

            var errorDataLockStatusesToGenerate = (int)(numberOfNewApprenticeships *
                                                          TestDataVolume.ErrorDataLockStatusesToApprenticeshipsRatio);

            //firstNewDataLockStatusUpdateId += testDataLockStatuses.Count;

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
            var fixture = new Fixture();//.Customize(new IntegrationTestCustomisation());
            //fixture.Customizations.Insert(0, new RandomEnumSequenceGenerator<TableType>())
            var apprenticeships = fixture.CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate).ToList();

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max
            //todo: get's id that isn't generated if rows are already generated!
            testIds[TestIds.MaxCohortSize] = initialId;

            int apprenticeshipsLeftInCohort = maxCohortSize;

            foreach (var apprenticeship in apprenticeships)
            {
                //apprenticeship.Id = initialId++; //todo: required?

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
            //// limit length? does it matter if lazily enumerated and don't read past required?
            //var newApprenticeshipIdsShuffled = Enumerable
            //    .Range((int) firstId, countOfIds)
            //    .OrderBy(au => _random.Next(int.MaxValue));

            //// limit length in aggregate? does it matter if lazily enumerated and don't read past required?
            //var apprenticeshipIdsForUpdates = newApprenticeshipIdsShuffled.Aggregate(Enumerable.Empty<int>(),
            //    (ids, id) => ids.Concat(Enumerable.Repeat(id, _random.Next(1, TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship+1))));

            var apprenticeshipIdsForUpdates = RandomIdGroups(firstNewApprenticeshipId, numberOfNewApprenticeships,
                TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship);

            var fixture = new Fixture();
            var apprenticeshipUpdates = fixture.CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();

            return apprenticeshipUpdates.Zip(apprenticeshipIdsForUpdates, (update, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                //update.Id = initialId++;
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

            //todo: looks like this may be blowing the stack for some reason (with large countOfIds)
            // it breaks in system code
            // An unhandled exception of type 'System.StackOverflowException' occurred in Unknown Module. occurred
            // call stack -> The number of stack frames exceeds the limit

            // limit length in aggregate? does it matter if lazily enumerated and don't read past required?
            //var apprenticeshipIdsForUpdates = newApprenticeshipIdsShuffled.Aggregate(Enumerable.Empty<int>(),
            //    (ids, id) => ids.Concat(Enumerable.Repeat(id, _random.Next(1, maxIdsPerGroup + 1))));

            //return apprenticeshipIdsForUpdates.Cast<long>();

            //todo: could work with shuffledIds directly (not zip them) if this blows the stack also

            return newApprenticeshipIdsShuffled.SelectMany(id => Enumerable.Repeat((long)id, Random.Next(1, maxIdsPerGroup + 1)));
        }

        public static List<DbSetupCommitment> GenerateCommitments(int commitmentsToGenerate)
        {
            var fixture = new Fixture();
            var commitments = fixture.CreateMany<DbSetupCommitment>(commitmentsToGenerate).ToList();
            foreach (var commitment in commitments)
            {
                //commitment.Id = initialId++;
                // we'll probably have to do better than this at some point, but this might be enough
                // for the initial tests
                // if we do something a bit closer to real-world, we'll have to add probably 2
                // extra columns to IntegrationTestIds, EmployerId & ProviderId (or possibly 1 column as a (c++ style) union)
                commitment.EmployerAccountId = commitment.Id;
            }
            return commitments;
        }

        public static List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdate(int apprenticeshipUpdatesToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var apprentieshipUpdates = fixture.CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();
            //foreach (var apprentieshipUpdate in apprentieshipUpdates)
            //{
            //    apprentieshipUpdate.Id = initialId++;
            //}
            return apprentieshipUpdates;
        }

        public List<DbSetupDataLockStatus> GenerateDataLockStatuses(IEnumerable<long> randomlyOrderedApprenticeshipIdGroups, int dataLockStatusesToGenerate, bool setError = false)
        {
            var fixture = new Fixture();
            var dataLockStatuses = fixture.CreateMany<DbSetupDataLockStatus>(dataLockStatusesToGenerate).ToList();

            //todo: don't use autofixture for these, just generate the whole lot by hand?
            return dataLockStatuses.Zip(randomlyOrderedApprenticeshipIdGroups, (dataLockStatus, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope. could create new
                //dataLockStatus.Id = initialId++;
                dataLockStatus.ApprenticeshipId = apprenticeshipId;
                dataLockStatus.Status = GenerateStatus(setError);
                dataLockStatus.ErrorCode = GenerateDataLockError(setError);
                dataLockStatus.TriageStatus = GenerateTriageStatus(dataLockStatus.ErrorCode);
                dataLockStatus.IsResolved = GenerateIsResolved(dataLockStatus.TriageStatus);
                dataLockStatus.EventStatus = GenerateEventStatus();
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
            //todo: which are the change codes?
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

        private EventStatus GenerateEventStatus()
        {
            // majority are removed
            //todo: pick these percentages from TestDataVolume?
            //todo: do we need to add certain apprenticeships to TestIds? do we need certain status sets? can we rely on db query to fetch?
            var rand = Random.Next(100);

            if (rand < 10)
                return EventStatus.New;

            if (rand < 20)
                return EventStatus.Updated;

            return EventStatus.Removed;
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
