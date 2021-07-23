using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class TransformFullCourseTitleExtensionsTests
    {
        [Test]
        public void When_CourseIsStandard_Then_StandardIsRemoved()
        {
            var courseTitle = "Standard title (Standard)";

            var result = courseTitle.TransformFullCourseTitle(ProgrammeType.Standard);

            Assert.AreEqual("Standard title", result);
        }

        [TestCase("Framework title")]
        [TestCase("Framework title ")]
        [TestCase("Framework title (Framework)")]
        public void When_CourseIsFramework_Then_AddFramework(string courseTitle)
        {
            var expectedTitle = "Framework title (Framework)";

            var result = courseTitle.TransformFullCourseTitle(ProgrammeType.Framework);

            Assert.AreEqual(expectedTitle, result);
        }
    }
}
