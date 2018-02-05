using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningAddEpaToApprenticeships
    {
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        }

        [Test]
        public void ThenSumfinkOrNuffink()
        {
        }
    }
}
