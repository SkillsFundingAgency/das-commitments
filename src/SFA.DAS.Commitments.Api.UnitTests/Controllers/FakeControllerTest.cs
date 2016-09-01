using NUnit.Framework;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers
{
    [TestFixture]
    public class FakeControllerTest
    {
        [Test]
        public void VerifyTrueIsNotFalse()
        {
            Assert.That(true, Is.True);
        }
    }
}