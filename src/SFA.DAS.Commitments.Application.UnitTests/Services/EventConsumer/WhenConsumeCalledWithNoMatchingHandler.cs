using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.EventConsumer
{
    [TestFixture]
    public class WhenConsumeCalledWithNoMatchingHandler
    {
        private Application.Services.EventConsumer _sut;

        [SetUp]
        public void Arrange()
        {
            _sut = new Application.Services.EventConsumer();
        }

        [Test]
        public async Task ThenTheHandlerIsNotCalled()
        {
            // arrange
            bool isCalled = false;

            _sut.RegisterHandler<TestClass1>((message) =>
            {
                isCalled = true;
                return Task.CompletedTask;
            });

            // act
            await _sut.Consume(new TestClass2());

            // assert
            Assert.IsFalse(isCalled);
        }

        private class TestClass1 { }
        private class TestClass2 { }

    }
}
