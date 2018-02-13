using System;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenPuttingApprenticeshipStopDate : EmployerOrchestratorTestBase
    {
        [SetUp]
        public void Arrange()
        {
            MockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipStopDateCommand>())).ReturnsAsync(new Unit());
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Arrange
            var stopDate = new ApprenticeshipStopDate
            {
                NewStopDate = DateTime.Today,
                LastUpdatedByInfo = new LastUpdateInfo(),
                UserId = "TEST"
            };

            //Act
            await Orchestrator.PutApprenticeshipStopDate(1, 1, 1, stopDate);
             
            //Assert
            MockMediator.Verify(x => x.SendAsync(It.IsAny<UpdateApprenticeshipStopDateCommand>()), Times.Once);
        }

    }
}
