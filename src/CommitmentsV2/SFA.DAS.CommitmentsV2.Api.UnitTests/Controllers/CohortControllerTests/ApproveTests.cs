using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class ApproveTests : FluentTest<ApproveTestsFixture>
    {
        [Test]
        public async Task WhenPostRequestReceived_ThenShouldReturnResponse()
        {
            await TestAsync(
                f => f.Approve(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkResult>());
        }
    }

    public class ApproveTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<IMediator> Mediator { get; set; }
        public CohortController Controller { get; set; }
        public ApproveCohortRequest Request { get; set; }

        private const long CohortId = 123;

        public ApproveTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);
            Request = AutoFixture.Create<ApproveCohortRequest>();

            Mediator.Setup(m => m.Send(It.Is<ApproveCohortCommand>(c =>
                c.CohortId == CohortId &&
                c.Message == Request.Message &&
                c.UserInfo == Request.UserInfo), CancellationToken.None));
        }

        public Task<IActionResult> Approve()
        {
            return Controller.Approve(CohortId, Request);
        }
    }
}