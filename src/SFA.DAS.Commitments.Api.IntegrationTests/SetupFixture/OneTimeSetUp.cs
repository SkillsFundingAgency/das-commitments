using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    [SetUpFixture]
    public class OneTimeSetUp
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await TestContext.Progress.WriteLineAsync("Running OneTimeSetUp");
            await TestData.PopulateDatabaseWithTestData();
        }
    }
}
