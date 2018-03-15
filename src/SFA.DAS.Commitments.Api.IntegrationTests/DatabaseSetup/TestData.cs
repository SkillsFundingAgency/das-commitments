using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    //todo: generating 1/4 million objects with autofixture is slow. speed it up!
    public class TestData
    {
        private readonly string _databaseConnectionString;
        public CommitmentsDatabase CommitmentsDatabase { get; }

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
            // do we want to support 'adding' to existing data, or make it so always populate from fresh db?
            // there are currently all sorts of edge cases for expansion
            // but if when deal with million+ rows and want to add another million might be handy to add more
            // could assert corner cases rather than trying to handle them?
            // if adding, would probably be better to leave current test ids alone and just generate for volume
            var firstNewApprenticeshipId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.ApprenticeshipTableName);
            var firstNewCohortId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.CommitmentTableName);

            var apprenticeshipsInTable = await CommitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName);

            //todo: this is usually in generate method
            var apprenticeshipsToGenerate = TestDataVolume.MinNumberOfApprenticeships - apprenticeshipsInTable;

            var testIds = new TestIds();

            if (apprenticeshipsToGenerate > 0)
            {
                (var testApprenticeships, long lastCohortId) = await new ApprenticeshipGenerator().Generate(
                    testIds, apprenticeshipsToGenerate, firstNewApprenticeshipId, firstNewCohortId);
                await CommitmentsDatabase.InsertApprenticeships(testApprenticeships);

                await CommitmentsDatabase.InsertCommitments(await new CommitmentGenerator().Generate(lastCohortId, firstNewCohortId));
            }

            await CommitmentsDatabase.InsertApprenticeshipUpdates(await new ApprenticeshipUpdateGenerator().Generate(apprenticeshipsToGenerate, firstNewApprenticeshipId));

            var firstNewDataLockStatusId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.DataLockStatusTableName);

            await CommitmentsDatabase.InsertDataLockStatuses(await new DataLockStatusGenerator().Generate(apprenticeshipsToGenerate, firstNewApprenticeshipId, firstNewDataLockStatusId));
            // need to Consume??
            return testIds;
        }
    }
}
