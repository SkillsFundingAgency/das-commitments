using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using System.Linq;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipWithdrawnEventHandlerTests
    {
        private Mock<ILogger<ApprenticeshipPriceChangedEventHandler>> _loggerMock;
        private Mock<IMediator> _mediatorMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ApprenticeshipPriceChangedEventHandler>>();
            _mediatorMock = new Mock<IMediator>();
        }

        [Test]
        public async Task Handle_Should_Call_WithdrawFromPaymentSimplificationBeta_When_Reason_Is_WithdrawFromBeta()
        {
            // Arrange
            var message = new ApprenticeshipWithdrawnEvent { ApprenticeshipId = 1, Reason = "WithdrawFromBeta" };
            var context = new Mock<IMessageHandlerContext>();
            var apprenticeship = new Apprenticeship { Id = 1, IsOnFlexiPaymentPilot = true };
            var mockDbContext = GetMockDbContext(apprenticeship);
            var handler = new ApprenticeshipWithdrawnEventHandler(_loggerMock.Object, new Lazy<ProviderCommitmentsDbContext>(() => mockDbContext.Object), _mediatorMock.Object);

            // Act
            await handler.Handle(message, context.Object);

            // Assert
            Assert.IsFalse(apprenticeship.IsOnFlexiPaymentPilot);
            mockDbContext.Verify(x => x.Update(apprenticeship), Times.Once);
            mockDbContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestCase("WithdrawFromStart")]
        [TestCase("WithdrawDuringLearning")]
        public async Task Handle_Should_Send_StopApprenticeshipCommand_When_Apprenticeship_Is_Fully_Withdrawn(string reason)
        {
            // Arrange
            var message = new ApprenticeshipWithdrawnEvent { ApprenticeshipId = 1, Reason = reason, EmployerAccountId = 2, ApprenticeshipKey = Guid.NewGuid(), LastDayOfLearning = new DateTime(2022, 7, 12)};
            var context = new Mock<IMessageHandlerContext>();
            var apprenticeship = new Apprenticeship { Id = 1, IsOnFlexiPaymentPilot = true };
            var mockDbContext = GetMockDbContext(apprenticeship);
            var handler = new ApprenticeshipWithdrawnEventHandler(_loggerMock.Object, new Lazy<ProviderCommitmentsDbContext>(() => mockDbContext.Object), _mediatorMock.Object);

            // Act
            await handler.Handle(message, context.Object);

            // Assert
            _mediatorMock.Verify(x => x.Send(It.Is<StopApprenticeshipCommand>(command =>
                command.AccountId == message.EmployerAccountId
                && command.ApprenticeshipId == message.ApprenticeshipId
                && command.StopDate == message.LastDayOfLearning
                && command.MadeRedundant == false
                && command.UserInfo.UserId == UserInfo.System.UserId
                && command.Party == Party.Employer
                ), It.IsAny<CancellationToken>()));
        }

        private Mock<ProviderCommitmentsDbContext> GetMockDbContext(Apprenticeship apprenticeship)
        {
            // Create a mock DbSet
            var apprenticeships = new List<Apprenticeship> { apprenticeship }.AsQueryable();

            var dbSetMock = new Mock<DbSet<Apprenticeship>>();
            dbSetMock.As<IQueryable<Apprenticeship>>()
                .Setup(m => m.Provider)
                .Returns(apprenticeships.Provider);
            dbSetMock.As<IQueryable<Apprenticeship>>()
                .Setup(m => m.Expression)
                .Returns(apprenticeships.Expression);
            dbSetMock.As<IQueryable<Apprenticeship>>()
                .Setup(m => m.ElementType)
                .Returns(apprenticeships.ElementType);
            dbSetMock.As<IQueryable<Apprenticeship>>()
                .Setup(m => m.GetEnumerator())
                .Returns(apprenticeships.GetEnumerator());

            // Setup DbContext to return the mock DbSet
            var dbContextMock = new Mock<ProviderCommitmentsDbContext>();
            dbContextMock.Setup(x => x.Apprenticeships).Returns(dbSetMock.Object);

            return dbContextMock;
        }
    }
}
