using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class SendTests : FluentTest<SendTestsFixture>
    {
        [Test]
        public async Task WhenPostRequestReceived_ThenShouldReturnResponse()
        {
            await TestAsync(
                f => f.Send(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkResult>());
        }
    }

    public class SendTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public Mock<IMediator> Mediator { get; set; }
        public Mock<IAuthenticationService> AuthenticationService { get; set; }
        public CohortController Controller { get; set; }
        public SendCohortRequest Request { get; set; }

        private const long CohortId = 123;
        
        public SendTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            AuthenticationService = new Mock<IAuthenticationService>();
            Controller = new CohortController(Mediator.Object, AuthenticationService.Object);
            Request = AutoFixture.Create<SendCohortRequest>();

            Mediator.Setup(m => m.Send(It.Is<SendCohortCommand>(c => 
                    c.CohortId == CohortId &&
                    c.Message == Request.Message &&
                    c.UserInfo == Request.UserInfo), CancellationToken.None))
                .ReturnsAsync(Unit.Value);
        }

        public Task<IActionResult> Send()
        {
            return Controller.Send(CohortId, Request);
        }
    }
}