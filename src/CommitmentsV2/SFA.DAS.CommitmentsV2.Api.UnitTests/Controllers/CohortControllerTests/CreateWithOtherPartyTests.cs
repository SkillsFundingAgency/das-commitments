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
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class CreateWithOtherPartyTests : FluentTest<CreateWithOtherPartyTestsFixture>
    {
        [Test]
        public async Task WhenPostRequestReceived_ThenShouldReturnResponse()
        {
            await TestAsync(
                f => f.Create(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().NotBeNull()
                    .And.Match<CreateCohortResponse>(v =>
                        v.CohortId == f.Result.Id &&
                        v.CohortReference == f.Result.Reference));
        }
    }

    public class CreateWithOtherPartyTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public CreateCohortWithOtherPartyRequest Request { get; }
        public AddCohortResult Result { get; }

        public CreateWithOtherPartyTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);
            Request = AutoFixture.Create<CreateCohortWithOtherPartyRequest>();
            Result = AutoFixture.Create<AddCohortResult>();

            Mediator
                .Setup(m => m.Send(It.Is<AddCohortWithOtherPartyCommand>(c =>
                    c.AccountId == Request.AccountId &&
                    c.AccountLegalEntityId == Request.AccountLegalEntityId &&
                    c.ProviderId == Request.ProviderId &&
                    c.TransferSenderId == Request.TransferSenderId &&
                    c.PledgeApplicationId == Request.PledgeApplicationId &&
                    c.Message == Request.Message &&
                    c.UserInfo == Request.UserInfo), CancellationToken.None))
                .ReturnsAsync(Result);
        }

        public Task<IActionResult> Create()
        {
            return Controller.Create(Request);
        }
    }
}