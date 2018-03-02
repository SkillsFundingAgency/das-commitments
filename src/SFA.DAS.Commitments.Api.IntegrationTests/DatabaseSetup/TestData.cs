using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Infrastructure.Data;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class TestData
    {
        private readonly CommitmentsApiConfiguration _config;
        private readonly CommitmentsDatabase _commitmentsDatabase;
        private readonly TestApprenticeshipIds _apprenticeshipIds;

        public TestData()
        {
            _config = Infrastructure.Configuration.Configuration.Get();

            _commitmentsDatabase = new CommitmentsDatabase(_config.DatabaseConnectionString);
            _apprenticeshipIds = new TestApprenticeshipIds();
        }

        //todo: handle case when database already populated & schema update (regenerate all data?)

        public async Task<TestApprenticeshipIds> Initialise()
        {
            //logger null gonna cause probs?
            //var jobProgressRepository = new JobProgressRepository(_config.DatabaseConnectionString, null);
            //jobProgressRepository.

            var existingDatabaseSchemaVersion = await _commitmentsDatabase.GetJobProgress<int?>("IntTest_SchemaVersion");
            if (existingDatabaseSchemaVersion != CommitmentsDatabase.SchemaVersion)
            {
                //todo:   truncate data
                return await PopulateDatabaseWithTestData();
            }

            // else if data in db
            //   load app ids
            // else populate data
            return await PopulateDatabaseWithTestData();

            
            // get schema
            // if schema in db not current
            //   truncate data
            //   populate data
            // else if data in db
            //   load app ids
            // else populate data

        }

        public async Task<TestApprenticeshipIds> PopulateDatabaseWithTestData()
        {
            var firstNewApprenticeshipId = await _commitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipTableName);
            var firstNewCohortId = await _commitmentsDatabase.FirstNewId(CommitmentsDatabase.CommitmentTableName);

            var apprenticeshipsInTable =
                await _commitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName);

            var apprenticeshipsToGenerate = TestDataVolume.MinNumberOfApprenticeships - apprenticeshipsInTable;

            (var testApprenticeships, long lastCohortId) = GenerateApprenticeships(apprenticeshipsToGenerate, firstNewApprenticeshipId, firstNewCohortId);
            await _commitmentsDatabase.InsertApprenticeships(testApprenticeships);

            // generate the commitments that the new apprenticeships reference
            int commitmentsToGenerate = (int)(1 + lastCohortId - firstNewCohortId);

            await TestContext.Progress.WriteLineAsync("Generating Commitments");

            var testCommitments = GenerateCommitments(commitmentsToGenerate, firstNewCohortId);
            await _commitmentsDatabase.InsertCommitments(testCommitments);

            await PopulateApprenticeshipUpdates(apprenticeshipsToGenerate, firstNewApprenticeshipId);

            await PopulateDatabaseWithDataLockStatuses();

            return _apprenticeshipIds;
        }

        private async Task PopulateApprenticeshipUpdates(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            //todo: methods for these
            // generate apprenticeship updates
            int apprenticeshipUpdatesToGenerate = (int)(apprenticeshipsGenerated * TestDataVolume.ApprenticeshipUpdatesToApprenticeshipsRatio);
            var firstNewApprenticeshipUpdateId = await _commitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipUpdateTableName);

            var testApprenticeshipUpdates = GenerateApprenticeshipUpdates(firstNewApprenticeshipId, apprenticeshipsGenerated, firstNewApprenticeshipUpdateId, apprenticeshipUpdatesToGenerate);
            await _commitmentsDatabase.InsertApprenticeshipUpdates(testApprenticeshipUpdates);
        }

        private async Task PopulateDatabaseWithDataLockStatuses()
        {
            // (for now at least) generate non-related datalockstatuses for bulking out the table
            const long bulkApprenticeshipId = long.MaxValue, bulkApprenticeshipUpdateId = long.MaxValue;
            var firstNewDataLockStatusUpdateId = await _commitmentsDatabase.FirstNewId(CommitmentsDatabase.DataLockStatusTableName);

            //for now..
            var dataLockStatusesToGenerate = TestDataVolume.MinNumberOfDataLockStatuses;
            var testDataLockStatuses = GenerateDataLockStatuses(bulkApprenticeshipId, bulkApprenticeshipUpdateId, dataLockStatusesToGenerate, firstNewDataLockStatusUpdateId);

            //todo: generate specific statuses for later tests
            //testDataLockStatuses.AddRange(GenerateDataLockStatuses(firstNewApprenticeshipId, bulkApprenticeshipUpdateId, commitmentsToGenerate, firstNewCohortId));
            await _commitmentsDatabase.InsertDataLockStatuses(testDataLockStatuses);
        }

        public (List<DbSetupApprenticeship>, long) GenerateApprenticeships(int apprenticeshipsToGenerate, long initialId = 1, long firstCohortId = 1, int maxCohortSize = TestDataVolume.MaxNumberOfApprenticeshipsInCohort)
        {
            var fixture = new Fixture();//.Customize(new IntegrationTestCustomisation());
            //fixture.Customizations.Insert(0, new RandomEnumSequenceGenerator<TableType>())
            var apprenticeships = fixture.CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate).ToList();

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max
            //todo: get's id that isn't generated if rows are already generated!
            _apprenticeshipIds.MaxCohortSize = initialId;

            var random = new Random();
            int apprenticeshipsLeftInCohort = maxCohortSize;

            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Id = initialId++; //todo: required?

                if (--apprenticeshipsLeftInCohort < 0)
                {
                    apprenticeshipsLeftInCohort = random.Next(1, maxCohortSize);
                    ++firstCohortId;
                }

                apprenticeship.CommitmentId = firstCohortId;
            }

            return (apprenticeships, firstCohortId);
        }

        public static List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdates(long firstNewApprenticeshipId, int numberOfNewApprenticeships, long initialId, int apprenticeshipUpdatesToGenerate)
        {
            var random = new Random();
            // limit length? does it matter if lazily enumerated and don't read past required?
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int) firstNewApprenticeshipId, numberOfNewApprenticeships)
                .OrderBy(au => random.Next(int.MaxValue));

            // limit length in aggregate? does it matter if lazily enumerated and don't read past required?
            var apprenticeshipIdsForUpdates = newApprenticeshipIdsShuffled.Aggregate(Enumerable.Empty<int>(),
                (ids, id) => ids.Concat(Enumerable.Repeat(id, random.Next(1, TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship))));

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

        public static List<DbSetupDataLockStatus> GenerateDataLockStatuses(long apprenticeshipId, long apprenticeshipUpdateId, int dataLockStatusesToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var dataLockStatuses = fixture.CreateMany<DbSetupDataLockStatus>(dataLockStatusesToGenerate).ToList();
            foreach (var dataLockStatus in dataLockStatuses)
            {
                dataLockStatus.Id = initialId++;
                dataLockStatus.ApprenticeshipId = apprenticeshipId;
                dataLockStatus.ApprenticeshipUpdateId = apprenticeshipUpdateId;
            }
            return dataLockStatuses;
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
