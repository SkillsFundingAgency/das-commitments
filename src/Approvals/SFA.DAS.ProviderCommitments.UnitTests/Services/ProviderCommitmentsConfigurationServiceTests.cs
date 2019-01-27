using Moq;
using NUnit.Framework;
using SFA.DAS.Configuration;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Interfaces;
using SFA.DAS.ProviderCommitments.Services;
using System;
using FluentAssertions;
using SFA.DAS.ProviderCommitments.Exceptions;

namespace SFA.DAS.ProviderCommitments.UnitTests.Services
{
    [TestFixture]
    public class ProviderCommitmentsConfigurationServiceTests
    {
        [Test]
        public void Constructor_Valid_ShouldNotThrowException()
        {
            var fixtures = new ProviderCommitmentsConfigurationServiceTestFixtures();

            fixtures.CreateService();

            Assert.Pass("Service constructed without exception");
        }

        [TestCase("{ 'ConfigClass1': {'Value':'HelloWorld'}}", typeof(ConfigClass1), typeof(ConfigClass1))]
        [TestCase("{ 'ConfigClass1': {'Value':'HelloWorld'}, 'ConfigClass2': {}}", typeof(ConfigClass1), typeof(ConfigClass1))]
        [TestCase("{ 'ConfigClass1': {'Value':'HelloWorld'}, 'ConfigClass2': {}}", typeof(ConfigClass2), typeof(ConfigClass2))]
        // The following config json is valid because the default behaviour of NewtonSoft is to keep the latest property value.
        [TestCase("{ 'ConfigClass1': {'Value': 'HelloWorld'}, 'ConfigClass1':{'Value': 'HelloUniverse'}}", typeof(ConfigClass1), typeof(ConfigClass1))]
        public void Get_WithValidType_ShouldReturnConfigType(
            string jsonConfig, 
            Type requestedType, 
            params Type[] matchingTypes)
        {
            var fixtures = new ProviderCommitmentsConfigurationServiceTestFixtures()
                .WithConfig(jsonConfig)
                .WithApplicationType(requestedType.Name, matchingTypes);
                                
            var configService = fixtures.CreateService();

            var config = configService.Get(requestedType);

            Assert.IsNotNull(config);
        }

        [Test]
        public void Get_WithoutConfigInJson_ShouldThrowMissingConfigException()
        {
            var fixtures = new ProviderCommitmentsConfigurationServiceTestFixtures()
                .WithConfig("{ 'ConfigClass1': {'Value':'HelloWorld'}}")
                .WithApplicationType(nameof(ConfigClass2), new []{typeof(ConfigClass2)});

            var configService = fixtures.CreateService();

            Assert.Throws<MissingConfigItemException>(() => configService.Get<ConfigClass2>());
        }

        [TestCase("{ 'ConfigClass1': {'Value':'HelloWorld'}}", typeof(ConfigClass1), ConfigObjectStatus.TypeNotFound)]
        [TestCase("{ 'ConfigClass1': {'Value':'HelloWorld'}}", typeof(ConfigClass1), ConfigObjectStatus.AmbiguousType, typeof(ConfigClass1), typeof(ConfigClass2))]
        [TestCase("{ 'ConfigClass1': {'Value': [1,2, 3]}}", typeof(ConfigClass1), ConfigObjectStatus.CouldNotBeDeserialised, typeof(ConfigClass1))]
        public void Get_WithInvalidConfig_ShouldThrowConfigItemUnavailableExceptionWithExpectedStatus(
            string jsonConfig,
            Type requestedType,
            ConfigObjectStatus expectedStatus,
            params Type[] matchingTypes)
        {
            var fixtures = new ProviderCommitmentsConfigurationServiceTestFixtures()
                .WithConfig(jsonConfig)
                .WithApplicationType(requestedType.Name, matchingTypes);

            var configService = fixtures.CreateService();

            var exception = Assert.Throws<ConfigItemUnavailableException>(() => configService.Get<ConfigClass1>());

            Assert.AreEqual(expectedStatus, exception.Status);
        }

        private class ConfigClass1
        {
            public string Value { get; set; }
        }

        private class ConfigClass2
        {
            public string Value { get; set; }
        }
    }


    public class ProviderCommitmentsConfigurationServiceTestFixtures
    {
        public ProviderCommitmentsConfigurationServiceTestFixtures()
        {
            AssemblyDiscoveryServiceMock = new Mock<IAssemblyDiscoveryService>();
            ConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            EnvironmentServiceMock = new Mock<IEnvironmentService>();
            LogMock = new Mock<ILog>();
        }

        public Mock<IAssemblyDiscoveryService> AssemblyDiscoveryServiceMock { get; }
        public IAssemblyDiscoveryService AssemblyDiscoveryService => AssemblyDiscoveryServiceMock.Object;


        public Mock<IConfigurationRepository> ConfigurationRepositoryMock { get; }
        public IConfigurationRepository ConfigurationRepository => ConfigurationRepositoryMock.Object;

        public Mock<IEnvironmentService> EnvironmentServiceMock { get; }
        public IEnvironmentService EnvironmentService => EnvironmentServiceMock.Object;

        public Mock<ILog> LogMock { get; }
        public ILog Log => LogMock.Object;



        public ProviderCommitmentsConfigurationService CreateService()
        {
            return new ProviderCommitmentsConfigurationService(
                ConfigurationRepository, 
                EnvironmentService, 
                AssemblyDiscoveryService,
                Log);
        }

        public ProviderCommitmentsConfigurationServiceTestFixtures WithConfig(string config)
        {
            // Allow defined config to use single quotes for double quotes to avoid all that escaping.
            var actualConfig = config.Replace('\'', '"');

            ConfigurationRepositoryMock
                .Setup(cs => cs.Get(Constants.ServiceName, It.IsAny<string>(), "1.0"))
                .Returns(config);

            return this;
        }

        public ProviderCommitmentsConfigurationServiceTestFixtures WithApplicationType(
            string whenRequestingClass, 
            Type[] returnsTypes)
        {
            AssemblyDiscoveryServiceMock
                .Setup(ads => ads.GetApplicationTypes(whenRequestingClass))
                .Returns(returnsTypes);

            return this;
        }
    }
}
