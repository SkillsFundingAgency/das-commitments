using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public abstract class UpdateApprenticeshipStatusBase
    {
        protected Mock<ICommitmentRepository> MockCommitmentRespository;
        protected Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected Mock<IApprenticeshipEvents> MockEventsApi;
        protected Mock<IHistoryRepository> MockHistoryRepository;
        protected Mock<IDataLockRepository> MockDataLockRepository;
        protected StopApprenticeshipCommandHandler Handler;
        protected Mock<IAcademicYearValidator> MockAcademicYearValidator;
        protected Mock<ICommitmentsLogger> MockCommitmentsLogger;

        /// <summary>
        /// Setup and mock the Unit depencencies
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {

            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();

            MockEventsApi = new Mock<IApprenticeshipEvents>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockDataLockRepository = new Mock<IDataLockRepository>();

            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            MockAcademicYearValidator = new Mock<IAcademicYearValidator>();


            MockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            Handler = new StopApprenticeshipCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new UpdateApprenticeshipStatusValidator(),
                MockCurrentDateTime.Object,
                MockEventsApi.Object,
                MockCommitmentsLogger.Object,
                MockHistoryRepository.Object,
                MockDataLockRepository.Object,
                MockAcademicYearValidator.Object);
        }
    }
}
