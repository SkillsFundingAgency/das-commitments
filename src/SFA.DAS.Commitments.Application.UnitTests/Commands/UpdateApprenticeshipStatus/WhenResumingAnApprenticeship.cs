using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public abstract class WhenResumingAnApprenticeship
    {
        protected ResumeApprenticeshipCommand ExampleValidRequest;
        protected ResumeApprenticeshipCommandHandler Handler;

        protected Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        protected Mock<ICommitmentRepository> MockCommitmentRespository;
        protected Mock<ICommitmentsLogger> MockCommitmentsLogger;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected Mock<IApprenticeshipEvents> MockEventsApi;
        protected Mock<IHistoryRepository> MockHistoryRepository;
        protected Apprenticeship TestApprenticeship;


        [SetUp]
        public virtual void SetUp()
        {
           
            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            MockEventsApi = new Mock<IApprenticeshipEvents>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            Handler = new ResumeApprenticeshipCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                MockCurrentDateTime.Object,
                MockEventsApi.Object,
                MockCommitmentsLogger.Object,
                MockHistoryRepository.Object);
        }
    }
}