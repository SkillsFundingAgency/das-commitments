using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStopDate
{
    public abstract class WhenUpdatingAnApprenticeshipStopDate
    {
        protected UpdateApprenticeshipStopDateCommand ExampleValidRequest;
        protected UpdateApprenticeshipStopDateCommandHandler Handler;
        protected Mock<IAcademicYearValidator> MockAcademicYearValidator;
        protected Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        protected Mock<ICommitmentRepository> MockCommitmentRespository;
        protected Mock<ICommitmentsLogger> MockCommitmentsLogger;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected Mock<IDataLockRepository> MockDataLockRepository;
        protected Mock<IHistoryRepository> MockHistoryRepository;
        protected Mock<IApprenticeshipEventsPublisher> MockApprenticeshipEventsPublisher;
        protected Apprenticeship TestApprenticeship;
        protected Mock<IV2EventsPublisher> MockV2EventsPublisher;

        [SetUp]
        public virtual void SetUp()
        {
            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockDataLockRepository = new Mock<IDataLockRepository>();
            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            MockCommitmentsLogger = new Mock<ICommitmentsLogger>();
            MockApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            MockV2EventsPublisher = new Mock<IV2EventsPublisher>();

            Handler = new UpdateApprenticeshipStopDateCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new UpdateApprenticeshipStopDateCommandValidator(),
                MockCurrentDateTime.Object,
                MockCommitmentsLogger.Object,
                MockHistoryRepository.Object,
                MockAcademicYearValidator.Object,
                MockApprenticeshipEventsPublisher.Object,
                new ApprenticeshipEventsList(),
                MockDataLockRepository.Object,
                MockV2EventsPublisher.Object);
        }
    }
}