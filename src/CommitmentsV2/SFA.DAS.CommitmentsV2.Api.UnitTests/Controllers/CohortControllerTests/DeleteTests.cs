using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class DeleteTests
    {
        [Test]
        public async Task WhenDeleteIsCalled_ThenShouldReturnNoContentResult()
        {
            var fixture = new DeleteTestsFixture();
            var response = await fixture.Send();

            Assert.That(response, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task WhenDeleteIsCalled_ThenShouldPassValuesToDeleteCommand()
        {
            var fixture = new DeleteTestsFixture();
            await fixture.Send();
            
            fixture.VerifyDeleteCohortCommandIsSentCorrectly();
        }

        public class DeleteTestsFixture
        {
            public IFixture AutoFixture { get; set; }
            public Mock<IMediator> Mediator { get; set; }
            public CohortController Controller { get; set; }
            public UserInfo UserInfo { get; set; }
            public CancellationToken CancellationToken { get; set; }

            private const long CohortId = 123;

            public DeleteTestsFixture()
            {
                AutoFixture = new Fixture();
                Mediator = new Mock<IMediator>();
                Controller = new CohortController(Mediator.Object);
                UserInfo = AutoFixture.Create<UserInfo>();

                Mediator.Setup(m => m.Send(It.IsAny<DeleteCohortCommand>(), It.IsAny<CancellationToken>()));
            }

            public Task<IActionResult> Send()
            {
                return Controller.Delete(CohortId, UserInfo, CancellationToken);
            }

            public void VerifyDeleteCohortCommandIsSentCorrectly()
            {
                Mediator.Verify(m => m.Send(It.Is<DeleteCohortCommand>(c =>
                    c.CohortId == CohortId &&
                    c.UserInfo == UserInfo), CancellationToken), Times.Once);
            }
        }
    }
}