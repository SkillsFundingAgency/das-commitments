using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Services;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CommitmentsServiceTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCreatingCommitmentsServiceTests
    {
        private CommitmentsServiceTestFixtures _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CommitmentsServiceTestFixtures();
        }

        [Test]
        public void Constructor_Valid_ShouldNotThrowException()
        {
            var result = new CommitmentsService(_fixture.CommitmentsApiClientMock.Object, _fixture.HashingServiceMock.Object);

            Assert.Pass("Did not get an exception");
        }

        [Test]
        public void Constructor_NullApiClient_ShouldShouldThrowNullArgumentException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CommitmentsService(null, _fixture.HashingServiceMock.Object));
            Assert.IsTrue(ex.Message.Contains("client", StringComparison.InvariantCultureIgnoreCase), ex.Message);
        }

        [Test]
        public void Constructor_NullEncodingService_ShouldShouldThrowNullArgumentException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CommitmentsService(_fixture.CommitmentsApiClientMock.Object, null));
            Assert.IsTrue(ex.Message.Contains("encodingservice", StringComparison.InvariantCultureIgnoreCase), ex.Message);
        }
    }
}
