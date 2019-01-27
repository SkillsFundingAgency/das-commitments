using Moq;
using NUnit.Framework;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Services;
using System;
using System.Linq;

namespace SFA.DAS.ProviderCommitments.UnitTests.Services
{
    [TestFixture]
    public class AssemblyDiscoveryServiceTests
    {
        [Test]
        public void Constructor_Valid_ShouldNotThrowException()
        {
            var applicationDiscoveryService = new AssemblyDiscoveryService();

            Assert.Pass("Service constructed without exception");
        }

        [TestCase("TestClass1", typeof(NameSpace1.TestClass1), typeof(NameSpace2.TestClass1))]
        [TestCase("NameSpace1.TestClass1", typeof(NameSpace1.TestClass1))]
        public void GetApplicationTypes_WithValidClassNames_ShouldReturnExpectedClasses(string matchingClass, params Type[] expectedTypes)
        {
            var applicationDiscoveryService = new AssemblyDiscoveryService();

            var types = applicationDiscoveryService.GetApplicationTypes("SFA.DAS.ProviderCommitments.UnitTests", matchingClass);

            Assert.AreEqual(expectedTypes.Length, types.Length);
            foreach (var expectedType in expectedTypes)
            {
                Assert.IsTrue(types.Contains(expectedType), $"expected type {expectedType} was not found");
            }
        }
    }
}

namespace SFA.DAS.ProviderCommitments.UnitTests.NameSpace1
{
    public class TestClass1
    {

    }

    public class TestClass2
    {

    }
}

namespace SFA.DAS.ProviderCommitments.UnitTests.NameSpace2
{
    public class TestClass1
    {

    }

    public class TestClass2
    {

    }
}

