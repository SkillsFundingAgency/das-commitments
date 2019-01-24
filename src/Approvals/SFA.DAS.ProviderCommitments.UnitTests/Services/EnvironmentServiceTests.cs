using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Services;

namespace SFA.DAS.ProviderCommitments.UnitTests.Services
{
    [TestFixture]
    public class EnvironmentServiceTests
    {
        [Test]
        public void Constructor_Valid_ShouldNotThrowException()
        {
            var fixtures = new EnvironmentServiceTestFixtures();

            fixtures.CreateService();

            Assert.Pass("Service constructed without exception");
        }

        [TestCase("At", "AT", EnvironmentType.AT)]
        [TestCase("Demo", "DEMO", EnvironmentType.Demo)]
        [TestCase("Local", "LOCAL", EnvironmentType.Local)]
        [TestCase("mo", "MO", EnvironmentType.ModelOffice)]
        [TestCase("pp", "PP", EnvironmentType.PreProd)]
        [TestCase("prd", "PRD", EnvironmentType.Production)]
        [TestCase("Test", "TEST", EnvironmentType.Test)]
        [TestCase("Test2", "TEST2", EnvironmentType.Test)]
        public void GetEnvName_WithValidEnvironmentSetByEnvironmentVariable_ShouldReturnCapitalisedNamed(
            string environmentName, 
            string expectedName, 
            EnvironmentType expectedEnvironmentType)
        {
            var fixtures = new EnvironmentServiceTestFixtures()
                                .WithEnvironmentSpecifiedByEnvironmentVariable(environmentName);

            var environmentService = fixtures.CreateService();

            Assert.AreEqual(expectedName, environmentService.EnvironmentName, "The environment name has not been interpreted correctly");
            Assert.AreEqual(expectedEnvironmentType, environmentService.EnvironmentType, "The environment name has not been mapped to an environment type correctly");
        }
    }

    public class EnvironmentServiceTestFixtures
    {
        public EnvironmentServiceTestFixtures()
        {
            LogMock = new Mock<ILog>();
        }

        public Mock<ILog> LogMock { get; }
        public ILog Log => LogMock.Object;

        public EnvironmentService CreateService()
        {
            return new EnvironmentService(Log);
        }

        public EnvironmentServiceTestFixtures WithEnvironmentSpecifiedByEnvironmentVariable(string environmentName)
        {
            Environment.SetEnvironmentVariable(Constants.EnvironmentVariableNames.EnvironmentName, environmentName);

            return this;
        }
    }
}
