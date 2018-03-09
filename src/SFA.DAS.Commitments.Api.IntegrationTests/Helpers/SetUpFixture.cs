using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        public static TestIds TestIds { get; private set; }
        public static CommitmentsDatabase CommitmentsDatabase { get; private set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            try
            {
                //Debug.Listeners.Add(new DefaultTraceListener());
                await LogProgress("Running OneTimeSetUp");
                var testData = new TestData();
                TestIds = await testData.Initialise();
                CommitmentsDatabase = testData.CommitmentsDatabase;
            }
            catch (Exception exception)
            {
                Assert.Fail($"OneTimeSetup threw exception: {exception}");
            }
        }

        [OneTimeTearDown]
        public void TearDownOneTimeTearDown()
        {
            IntegrationTestServer.Shutdown();
        }

        public static async Task LogProgress(string message)
        {
            // https://stackoverflow.com/questions/41877586/nunit-text-output-not-behaving
            await TestContext.Progress.WriteLineAsync(message);

            // error should output immediately: https://github.com/nunit/nunit/issues/1139
            //await TestContext.Error.WriteLineAsync(message);

            //https://stackoverflow.com/questions/15092438/using-resharper-how-to-show-debug-output-during-a-long-running-unit-test

            //TestContext.WriteLine("TestContext.WriteLine");
            //TestContext.Error.WriteLine("TestContext.Error.WriteLine");
            //TestContext.Out.WriteLine("TestContext.Out.WriteLine");
            //TestContext.Progress.WriteLine("TestContext.Progress.WriteLine");
            //await TestContext.Error.WriteLineAsync("TestContext.Error.WriteLineAsync");
            //await TestContext.Out.WriteLineAsync("TestContext.Out.WriteLineAsync");
            //await TestContext.Progress.WriteLineAsync("TestContext.Progress.WriteLineAsync");
            //Trace.TraceInformation("Trace.TraceInformation");
            //Debug.WriteLine("Debug.WriteLine");
            //Console.WriteLine("Console.WriteLine");
        }
    }
}
