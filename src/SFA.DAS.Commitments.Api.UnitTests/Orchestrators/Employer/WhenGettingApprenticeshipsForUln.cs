using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingApprenticeshipsForUln : EmployerOrchestratorTestBase
    {
        private const string TestUln = "6791776799";

        [Test]
        public async Task ShouldReturnsEmptyResult()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .ReturnsAsync(new GetActiveApprenticeshipsByUlnResponse { Data = new List<ApprenticeshipResult> { } });

            var result = await Orchestrator.GetActiveApprenticeshipsForUln(1L, TestUln);

            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldSendAppropriateRequestToMediator()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .ReturnsAsync(new GetActiveApprenticeshipsByUlnResponse { Data = new List<ApprenticeshipResult> { } });

            await Orchestrator.GetActiveApprenticeshipsForUln(1L, TestUln);

            var expectedCommand = typeof(GetActiveApprenticeshipsByUlnRequest);

            MockMediator.Verify(x => x.SendAsync(It.Is<GetActiveApprenticeshipsByUlnRequest>(o => o.GetType() == expectedCommand)), Times.Once);
        }

        [Test]
        public async Task ShouldNotFilterAnyRecords()
        {
            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .ReturnsAsync(new GetActiveApprenticeshipsByUlnResponse
                {
                    Data = new List<ApprenticeshipResult>
                            {
                                new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active },
                                new ApprenticeshipResult { PaymentStatus = PaymentStatus.PendingApproval },
                                new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active },
                                new ApprenticeshipResult { PaymentStatus = PaymentStatus.Completed },
                                new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active }
                            }
                });

            var result = await Orchestrator.GetActiveApprenticeshipsForUln(1L, TestUln);

            result.Count().Should().Be(5);
        }
    }
}
