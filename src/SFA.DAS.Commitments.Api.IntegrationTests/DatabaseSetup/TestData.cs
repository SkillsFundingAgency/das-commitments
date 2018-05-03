using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests.API;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    //todo: generating 1/4 million objects with autofixture is slow. speed it up!
    public class TestData
    {
        private readonly string _databaseConnectionString;
        public CommitmentsDatabase CommitmentsDatabase { get; }

        public TestData(CommitmentsApiConfiguration config)
        {
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
                // we still need to set the schema version, to handle the cases where...
                // someone has manually deleted the db or we're running in a new environment
                await CommitmentsDatabase.SetJobProgress(schemaVersionColumnName, CommitmentsDatabase.SchemaVersion);
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
            //todo: if adding, would probably be better to leave current test ids alone and just generate for volume

            var testIds = new TestIds();    //todo: injector could take ownership of testids
            var testDataInjector = await TestDataInjector.Construct(CommitmentsDatabase, testIds);

            GatherTestSpecificData(testDataInjector);

            //todo: use entities contained in testSpecificData first (ideally interspersed) before generating random volume data
            //      and store the id of any entity given a name into testids

            var apprenticeshipsInTable = await CommitmentsDatabase.CountOfRows(CommitmentsDatabase.ApprenticeshipTableName);

            //todo: this is usually in generate method
            var apprenticeshipsToGenerate = TestDataVolume.MinNumberOfApprenticeships - apprenticeshipsInTable;

            if (apprenticeshipsToGenerate > 0)
            {
                (var testApprenticeships, long lastCohortId) = await new ApprenticeshipGenerator().Generate(
                    testIds, apprenticeshipsToGenerate, testDataInjector);
                await CommitmentsDatabase.InsertApprenticeships(testApprenticeships);

                await CommitmentsDatabase.InsertCommitments(await new CommitmentGenerator().Generate(lastCohortId, testDataInjector));
            }

            await CommitmentsDatabase.InsertApprenticeshipUpdates(await new ApprenticeshipUpdateGenerator().Generate(apprenticeshipsToGenerate, testDataInjector.FirstApprenticeshipId));

            // the DataLockStatus table diverges from the other tables by having its own id column seperate from the identity 'Id' column
            var firstNewDataLockEventId = await CommitmentsDatabase.FirstNewId(CommitmentsDatabase.DataLockStatusTableName, "DataLockEventId");

            await CommitmentsDatabase.InsertDataLockStatuses(await new DataLockStatusGenerator().Generate(apprenticeshipsToGenerate, testDataInjector.FirstApprenticeshipId, firstNewDataLockEventId));

            return testIds;
        }

        public void GatherTestSpecificData(TestDataInjector testDataInjector)
        {
            // *** add your call to inject the specific data your integration test needs here ***
            GetApprenticeships.InjectTestSpecificData(testDataInjector);
        }
    }
}
