using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.UnitTests
{
    [TestFixture]
    public class WhenRunningJob
    {
        private Mock<IAcademicYearEndExpiryProcessor> _academicYearProcessor;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IAcademicYearDateProvider> _academicYearProvider;
        private Mock<ILog> _logger;
        private Job _sut;

        [SetUp]
        public void SetUp()
        {
            _academicYearProcessor = new Mock<IAcademicYearEndExpiryProcessor>();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _academicYearProvider = new Mock<IAcademicYearDateProvider>();
            _logger = new Mock<ILog>();

            _sut = new Job(_academicYearProcessor.Object, _currentDateTime.Object, _academicYearProvider.Object, _logger.Object);
        }

        [TestCase("2017-10-20 19:00:00")]
        [TestCase("2017-10-19 18:00:00")]
        [TestCase("2018-01-15 19:00:00")]
        [TestCase("2018-07-31 21:00:00")]
        [TestCase("2018-09-15 21:00:00")]
        public void ShouldRun(DateTime dateNow)
        {
            _currentDateTime.Setup(m => m.Now).Returns(dateNow);
            _academicYearProvider.Setup(m => m.LastAcademicYearFundingPeriod).Returns(new DateTime(2017, 10, 19, 18, 00, 0));

            _sut.Run();

            _academicYearProcessor.Verify(m => m.RunDataLock(It.IsAny<string>()), Times.Once);
            _academicYearProcessor.Verify(m => m.RunApprenticeshipUpdateJob(It.IsAny<string>()), Times.Once);
        }

        [TestCase("2017-10-18 19:00:00")]
        [TestCase("2017-10-19 17:59:59")]
        [TestCase("2017-09-15 19:00:00")]
        [TestCase("2017-07-15 19:00:00")]
        public void ShouldNotRun(DateTime dateNow)
        {
            _currentDateTime.Setup(m => m.Now).Returns(dateNow);
            _academicYearProvider.Setup(m => m.LastAcademicYearFundingPeriod).Returns(new DateTime(2017, 10, 19, 18, 00, 0));

            _sut.Run();

            _academicYearProcessor.Verify(m => m.RunDataLock(It.IsAny<string>()), Times.Never);
            _academicYearProcessor.Verify(m => m.RunApprenticeshipUpdateJob(It.IsAny<string>()), Times.Never);
        }
    }
}
