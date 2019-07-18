using System;
using System.Threading.Tasks;
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
        public Task Map_CourseCode_ShouldBeSet()
        {
            var courseCode = _autoFixture.Create<string>();
            return AssertPropertySet(input => input.Setup(x => x.Id).Returns(courseCode),
                output => output.CourseCode == courseCode);
        }

        [Test]
        public Task Map_Name_ShouldBeSet()
        {
            var name = _autoFixture.Create<string>();
            return AssertPropertySet(input => input.Setup(x => x.ExtendedTitle).Returns(name),
                output => output.Name == name);
        }

        [TestCase(Apprenticeships.Api.Types.ProgrammeType.Framework, ProgrammeType.Framework)]
        [TestCase(Apprenticeships.Api.Types.ProgrammeType.Standard, ProgrammeType.Standard)]
        public Task Map_ProgrammeType_ShouldBeSet(Apprenticeships.Api.Types.ProgrammeType progType,
            ProgrammeType outputType)
        {
            return AssertPropertySet(input => input.Setup(x => x.ProgrammeType).Returns(progType),
                output => output.ProgrammeType == outputType);
        }

        [Test]
        public Task Map_EffectiveFrom_ShouldBeSet()
        {
            var effectiveFromDate = _autoFixture.Create<DateTime?>();
            return AssertPropertySet(input => input.Setup(x => x.EffectiveFrom).Returns(effectiveFromDate),
                output => output.EffectiveFrom == effectiveFromDate);
        }

        [Test]
        public Task Map_EffectiveTo_ShouldBeSet()
        {
            var effectiveToDate = _autoFixture.Create<DateTime?>();
            return AssertPropertySet(input => input.Setup(x => x.EffectiveTo).Returns(effectiveToDate),
                output => output.EffectiveTo == effectiveToDate);
        }

        private async Task AssertPropertySet(Action<Mock<ITrainingProgramme>> setInput,
            Func<TrainingProgramme, bool> expectOutput)
        {
            var mapper = new ITrainingProgrammeToTrainingProgrammeMapper();

            var input = new Mock<ITrainingProgramme>();
            input.Setup(x => x.ProgrammeType).Returns(Apprenticeships.Api.Types.ProgrammeType.Framework);

            setInput.Invoke(input);

            var output = await mapper.Map(input.Object);

            Assert.IsTrue(expectOutput(output));
        }
    }
}