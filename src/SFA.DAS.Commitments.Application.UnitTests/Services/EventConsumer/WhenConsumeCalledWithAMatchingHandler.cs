using System.Threading.Tasks;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.EventConsumer
{
    [TestFixture]
    public class WhenConsumeCalledWithAMatchingHandler
    {
        private Application.Services.EventConsumer _sut;

        [SetUp]
        public void Arrange()
        {
            _sut = new Application.Services.EventConsumer();
        }

        [Test]
        public async Task ThenTheHandlerIsCalled()
        {
            // arrange
            var testMessage = new TestClass1();
            bool class1Called = false;
            bool class2Called = false;

            _sut.RegisterHandler<TestClass1>((message) =>
            {
                Assert.AreSame(testMessage, message);
                class1Called = true;
                return Task.CompletedTask;
            });

            _sut.RegisterHandler<TestClass2>((message) =>
            {
                Assert.AreSame(testMessage, message);
                class2Called = true;
                return Task.CompletedTask;
            });

            // act
            await _sut.Consume(testMessage);

            // assert
            Assert.IsTrue(class1Called);
            Assert.IsFalse(class2Called);
        }

        private class TestClass1 { }
        private class TestClass2 { }
    }
}
