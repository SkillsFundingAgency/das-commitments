using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.LearnerTests
{
    public class WhenConvertingFromDbLeaner
    {
        [Test, AutoData]
        public void ThenPopulatesModelFromDbLearner(Learner sut)
        {
            CommitmentsV2.Api.Types.Responses.Learner actual = sut;

            actual.Should().BeEquivalentTo(sut);
        }
    }

}
