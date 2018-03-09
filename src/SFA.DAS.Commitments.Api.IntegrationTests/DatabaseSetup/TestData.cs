using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Infrastructure.Data;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class TestData
    {
        //private readonly CommitmentsApiConfiguration _config;
        private readonly string _databaseConnectionString;
        public CommitmentsDatabase CommitmentsDatabase { get; }
        //private readonly TestIds _ids;
        //do we want to bother wrapping this dictionary? would also contain names
        //private readonly Dictionary<string, long> _ids = new Dictionary<string, long>();

        private readonly Random _random = new Random();

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

            //todo: handle case when database hasn't been created yet

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
                    databaseManagement.Kill();

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

                await TestContext.Progress.WriteLineAsync("Generating Commitments");

                var testCommitments = GenerateCommitments(commitmentsToGenerate, firstNewCohortId);
                await CommitmentsDatabase.InsertCommitments(testCommitments);
            }

            await PopulateApprenticeshipUpdates(apprenticeshipsToGenerate, firstNewApprenticeshipId);

            await PopulateDatabaseWithDataLockStatuses();

            return testIds;
        }

        private async Task PopulateApprenticeshipUpdates(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            //todo: methods for these
            // generate apprenticeship updates
            int apprenticeshipUpdatesToGenerate = (int)(apprenticeshipsGenerated * TestDataVolume.ApprenticeshipUpdatesToApprenticeshipsRatio);
            var firstNewApprenticeshipUpdateId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipUpdateTableName);

            var testApprenticeshipUpdates = GenerateApprenticeshipUpdates(firstNewApprenticeshipId, apprenticeshipsGenerated, firstNewApprenticeshipUpdateId, apprenticeshipUpdatesToGenerate);
            await CommitmentsDatabase.InsertApprenticeshipUpdates(testApprenticeshipUpdates);
        }

        private async Task PopulateDatabaseWithDataLockStatuses()
        {
            // (for now at least) generate non-related datalockstatuses for bulking out the table
            const long bulkApprenticeshipId = long.MaxValue, bulkApprenticeshipUpdateId = long.MaxValue;
            var firstNewDataLockStatusUpdateId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.DataLockStatusTableName);

            //for now..
            //todo: generate specific statuses for later tests

            // generate success DataLockStatuses
            var successDataLockStatusesToGenerate = (int)(TestDataVolume.MinNumberOfApprenticeships *
                    TestDataVolume.SuccessDataLockStatusesToApprenticeshipsRatio);
            var testDataLockStatuses = GenerateDataLockStatuses(bulkApprenticeshipId, bulkApprenticeshipUpdateId, successDataLockStatusesToGenerate, firstNewDataLockStatusUpdateId);

            //var errorDataLockStatusesToGenerate = (int)(TestDataVolume.MinNumberOfApprenticeships *
            //                                              TestDataVolume.ErrorDataLockStatusesToApprenticeshipsRatio);

            //testDataLockStatuses.AddRange(GenerateDataLockStatuses(firstNewApprenticeshipId, bulkApprenticeshipUpdateId, commitmentsToGenerate, firstNewCohortId));
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
                apprenticeship.Id = initialId++; //todo: required?

                if (--apprenticeshipsLeftInCohort < 0)
                {
                    apprenticeshipsLeftInCohort = _random.Next(1, maxCohortSize+1);
                    ++firstCohortId;
                }

                apprenticeship.CommitmentId = firstCohortId;
            }

            return (apprenticeships, firstCohortId);
        }

        public List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdates(long firstNewApprenticeshipId, int numberOfNewApprenticeships, long initialId, int apprenticeshipUpdatesToGenerate)
        {
            // limit length? does it matter if lazily enumerated and don't read past required?
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int) firstNewApprenticeshipId, numberOfNewApprenticeships)
                .OrderBy(au => _random.Next(int.MaxValue));

            // limit length in aggregate? does it matter if lazily enumerated and don't read past required?
            var apprenticeshipIdsForUpdates = newApprenticeshipIdsShuffled.Aggregate(Enumerable.Empty<int>(),
                (ids, id) => ids.Concat(Enumerable.Repeat(id, _random.Next(1, TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship+1))));

            var fixture = new Fixture();
            var apprenticeshipUpdates = fixture.CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();

            return apprenticeshipUpdates.Zip(apprenticeshipIdsForUpdates, (update, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                update.Id = initialId++;
                update.ApprenticeshipId = apprenticeshipId;
                return update;
            }).ToList();
        }

        public static List<DbSetupCommitment> GenerateCommitments(int commitmentsToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var commitments = fixture.CreateMany<DbSetupCommitment>(commitmentsToGenerate).ToList();
            foreach (var commitment in commitments)
            {
                commitment.Id = initialId++;
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
            foreach (var apprentieshipUpdate in apprentieshipUpdates)
            {
                apprentieshipUpdate.Id = initialId++;
            }
            return apprentieshipUpdates;
        }

        public List<DbSetupDataLockStatus> GenerateDataLockStatuses(long apprenticeshipId, long apprenticeshipUpdateId, int dataLockStatusesToGenerate, long initialId = 1, bool setError = false)
        {
            var fixture = new Fixture();
            var dataLockStatuses = fixture.CreateMany<DbSetupDataLockStatus>(dataLockStatusesToGenerate).ToList();
            foreach (var dataLockStatus in dataLockStatuses)
            {
                dataLockStatus.Id = initialId++;
                dataLockStatus.ApprenticeshipId = apprenticeshipId;
                dataLockStatus.ApprenticeshipUpdateId = apprenticeshipUpdateId;
                dataLockStatus.ErrorCode = setError ? GenerateDataLockError() : DataLockErrorCode.None;
            }
            return dataLockStatuses;
        }

        private DataLockErrorCode GenerateDataLockError()
        {
            var numberOfFlags = _random.Next(1, 3+1);
            int errorCode = 0;
            while (numberOfFlags-- > 0)
            {
                errorCode |= 1 << _random.Next(9+1);
            }
            return (DataLockErrorCode)errorCode;
        }

        //private static async Task<long> FirstNewId(CommitmentsDatabase commitmentsDatabase, string tableName)
        //{
        //    var latestIdInDatabase = await commitmentsDatabase.LastId(tableName);
        //    return (latestIdInDatabase ?? 0) + 1;
        //}
    }

    //public class RandomEnumSequenceGenerator<T> : ISpecimenBuilder where T : struct
    //{
    //    private static Random _random = new Random();
    //    private Array _values;

    //    public RandomEnumSequenceGenerator()
    //    {
    //        if (!typeof(T).IsEnum)
    //        {
    //            throw new ArgumentException("T must be an enum");
    //        }
    //        _values = Enum.GetValues(typeof(T));
    //    }

    //    public object Create(object request, ISpecimenContext context)
    //    {
    //        var t = request as Type;
    //        if (t == null || t != typeof(T))
    //            return new NoSpecimen();

    //        var index = _random.Next(0, _values.Length - 1);
    //        return _values.GetValue(index);
    //    }
    //}
}
