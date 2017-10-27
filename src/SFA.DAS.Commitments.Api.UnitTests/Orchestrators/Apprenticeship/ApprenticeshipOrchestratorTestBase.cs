using System.Collections.Generic;

using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    public class ApprenticeshipOrchestratorTestBase
    {
        protected ApprenticeshipsOrchestrator Orchestrator;

        protected Mock<IMediator> MockMediator;
        protected Mock<IDataLockMapper> MockDataLockMapper;
        protected Mock<IApprenticeshipMapper> MockApprenticeshipMapper;

        [SetUp]
        public void SetUp()
        {
            MockMediator = new Mock<IMediator>();
            MockDataLockMapper = new Mock<IDataLockMapper>();
            MockApprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            Orchestrator = new ApprenticeshipsOrchestrator(
                MockMediator.Object,
                MockDataLockMapper.Object,
                MockApprenticeshipMapper.Object,
                Mock.Of<ICommitmentsLogger>());

            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Apprenticeships = new List<Domain.Entities.Apprenticeship>
                            {
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active },
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.PendingApproval },
                                new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Paused }
                            }
                });
        }

    }


}