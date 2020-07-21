using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public abstract class WhenStoppingAnApprenticeship
    {
        protected StopApprenticeshipCommand ExampleValidRequest;
        protected StopApprenticeshipCommandHandler Handler;
        protected Mock<IAcademicYearValidator> MockAcademicYearValidator;
        protected Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        protected Mock<ICommitmentRepository> MockCommitmentRespository;
        protected Mock<ICommitmentsLogger> MockCommitmentsLogger;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected Mock<IDataLockRepository> MockDataLockRepository;
        protected Mock<IApprenticeshipEvents> MockEventsApi;
        protected Mock<IHistoryRepository> MockHistoryRepository;
        protected Mock<IV2EventsPublisher> MockV2EventsPublisher;
        protected Apprenticeship TestApprenticeship;

        [SetUp]
        public virtual void SetUp()
        {
            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            MockEventsApi = new Mock<IApprenticeshipEvents>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockDataLockRepository = new Mock<IDataLockRepository>();
            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            MockCommitmentsLogger = new Mock<ICommitmentsLogger>();
            MockV2EventsPublisher = new Mock<IV2EventsPublisher>();

            Handler = new StopApprenticeshipCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                MockCurrentDateTime.Object,
                MockEventsApi.Object,
                MockCommitmentsLogger.Object,
                MockHistoryRepository.Object,
                MockDataLockRepository.Object,
                MockV2EventsPublisher.Object);
        }
    }
}