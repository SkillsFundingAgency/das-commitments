using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;
using SFA.DAS.CommitmentsV2.Authentication;
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
            var f = new DeleteTestsFixture();
            var response = await f.Send();

            Assert.IsInstanceOf<NoContentResult>(response);
        }

        [Test]
        public async Task WhenDeleteIsCalled_ThenShouldPassValuesToDeleteCommand()
        {
            var f = new DeleteTestsFixture();
            await f.Send();
            f.VerifyDeleteCohortCommandIsSentCorrectly();
        }

        public class DeleteTestsFixture
        {
            public IFixture AutoFixture { get; set; }
            public Mock<IMediator> Mediator { get; set; }
            public Mock<IAuthenticationService> AuthenticationService { get; set; }
            public CohortController Controller { get; set; }
            public UserInfo UserInfo { get; set; }
            public CancellationToken CancellationToken { get; set; }

            private const long CohortId = 123;

            public DeleteTestsFixture()
            {
                AutoFixture = new Fixture();
                Mediator = new Mock<IMediator>();
                AuthenticationService = new Mock<IAuthenticationService>();
                Controller = new CohortController(Mediator.Object, AuthenticationService.Object);
                UserInfo = AutoFixture.Create<UserInfo>();

                Mediator.Setup(m => m.Send(It.IsAny<DeleteCohortCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value);
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