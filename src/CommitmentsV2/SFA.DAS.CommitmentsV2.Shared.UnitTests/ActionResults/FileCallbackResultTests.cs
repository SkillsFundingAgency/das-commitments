using SFA.DAS.CommitmentsV2.Shared.ActionResults;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.ActionResults
{
    public class FileCallbackResultTests
    {
        [Test]
        public void Then_If_The_Media_Type_Is_Not_Correct_An_Exception_Is_Thrown()
        {
            Assert.Throws<FormatException>(() => new FileCallbackResult("wrong", delegate{ return null; }));
        }

        [Test]
        public void Then_If_There_Is_No_Callback_An_Exception_Is_Thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new FileCallbackResult("text/csv", null));
        }

        [Test]
        public void Then_If_There_Is_No_Context_An_Exception_Is_Thrown()
        {
            var actionCallbackResult = new FileCallbackResult("text/csv", delegate { return null; });

            Assert.Throws<ArgumentNullException>(() => actionCallbackResult.ExecuteResultAsync(null));
        }

    }
}
