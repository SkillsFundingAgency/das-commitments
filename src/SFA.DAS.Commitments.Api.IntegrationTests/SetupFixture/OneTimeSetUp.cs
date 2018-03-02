//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
//using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
//using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

//namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
//{
//    [SetUpFixture]
//    public class SetUpFixure
//    {
//        public static TestApprenticeshipIds TestApprenticeshipIds { get; set; }

//        [OneTimeSetUp]
//        public async Task OneTimeSetUp()
//        {
//            await TestContext.Progress.WriteLineAsync("Running OneTimeSetUp");
//            TestApprenticeshipIds = await new TestData().PopulateDatabaseWithTestData();

//            // how to get app ids out?
//            // turn this into a base class?
//            // have as static in testdata? if so, testdata can go back to a static class.
//            // statis in this class?
//            // return from populatedb...?
//        }

//        [OneTimeTearDown]
//        public void TearDownOneTimeTearDown()
//        {
//            IntegrationTestServer.Shutdown();
//        }
//    }
//}
