using System;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Mapping;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    public class ITrainingProgrammeToTrainingProgrammeMapperTests
    {
        private Fixture _autoFixture;

        [SetUp]
        public void SetUp()
        {
            _autoFixture = new Fixture();
        }

        [Test]
        public void Map_CourseCode_ShouldBeSet()
        {
            var courseCode = _autoFixture.Create<string>();
            AssertPropertySet(input => input.Setup(x => x.Id).Returns(courseCode), output => output.CourseCode == courseCode);
        }

        [Test]
        public void Map_Name_ShouldBeSet()
        {
            var name = _autoFixture.Create<string>();
            AssertPropertySet(input => input.Setup(x => x.ExtendedTitle).Returns(name), output => output.Name == name);
        }

        [TestCase(Apprenticeships.Api.Types.ProgrammeType.Framework, ProgrammeType.Framework)]
        [TestCase(Apprenticeships.Api.Types.ProgrammeType.Standard, ProgrammeType.Standard)]
        public void Map_ProgrammeType_ShouldBeSet(Apprenticeships.Api.Types.ProgrammeType progType, ProgrammeType outputType)
        {
            AssertPropertySet(input => input.Setup(x => x.ProgrammeType).Returns(progType), output => output.ProgrammeType == outputType);
        }

        [Test]
        public void Map_EffectiveFrom_ShouldBeSet()
        {
            var effectiveFromDate = _autoFixture.Create<DateTime?>();
            AssertPropertySet(input => input.Setup(x => x.EffectiveFrom).Returns(effectiveFromDate), output => output.EffectiveFrom == effectiveFromDate);
        }

        [Test]
        public void Map_EffectiveTo_ShouldBeSet()
        {
            var effectiveToDate = _autoFixture.Create<DateTime?>();
            AssertPropertySet(input => input.Setup(x => x.EffectiveTo).Returns(effectiveToDate), output => output.EffectiveTo == effectiveToDate);
        }

        private void AssertPropertySet(Action<Mock<ITrainingProgramme>> setInput, Func<TrainingProgramme, bool> expectOutput)
        {
            var mapper = new ITrainingProgrammeToTrainingProgrammeMapper();

            var input = new Mock<ITrainingProgramme>();
            input.Setup(x=>x.ProgrammeType).Returns(Apprenticeships.Api.Types.ProgrammeType.Framework);

            setInput.Invoke(input);

            var output = mapper.Map(input.Object);

            Assert.IsTrue(expectOutput(output));
        }
    }
}
