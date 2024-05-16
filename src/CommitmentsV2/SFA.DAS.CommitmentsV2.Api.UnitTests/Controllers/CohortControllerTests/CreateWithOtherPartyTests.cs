using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.CohortControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class CreateWithOtherPartyTests
    {
        [Test]
        public async Task WhenPostRequestReceived_ThenShouldReturnResponse()
        {
            var fixture = new CreateWithOtherPartyTestsFixture();
            var result = await fixture.Create();
            
            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull()
                .And.Match<CreateCohortResponse>(response =>
                    response.CohortId == fixture.Result.Id &&
                    response.CohortReference == fixture.Result.Reference);
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