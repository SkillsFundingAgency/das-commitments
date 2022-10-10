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
    public class CreateTests : FluentTest<CreateTestsFixture>
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

    public class CreateTestsFixture
    {
        public IFixture AutoFixture { get; }
        public Mock<IMediator> Mediator { get; }
        public CohortController Controller { get; }
        public CreateCohortRequest Request { get; }
        public AddCohortResult Result { get; }

        public CreateTestsFixture()
        {
            AutoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            Controller = new CohortController(Mediator.Object);
            Request = AutoFixture.Create<CreateCohortRequest>();
            Result = AutoFixture.Create<AddCohortResult>();

            Mediator
                .Setup(m => m.Send(It.Is<AddCohortCommand>(c =>
                    c.AccountId == Request.AccountId &&
                    c.AccountLegalEntityId == Request.AccountLegalEntityId &&
                    c.ProviderId == Request.ProviderId &&
                    c.CourseCode == Request.CourseCode &&
                    c.DeliveryModel == Request.DeliveryModel &&
                    c.Cost == Request.Cost &&
                    c.StartDate == Request.StartDate &&
                    c.ActualStartDate == Request.ActualStartDate &&
                    c.EndDate == Request.EndDate &&
                    c.OriginatorReference == Request.OriginatorReference &&
                    c.ReservationId == Request.ReservationId &&
                    c.FirstName == Request.FirstName &&
                    c.LastName == Request.LastName &&
                    c.Email == Request.Email &&
                    c.DateOfBirth == Request.DateOfBirth &&
                    c.Uln == Request.Uln &&
                    c.TransferSenderId == Request.TransferSenderId &&
                    c.PledgeApplicationId == Request.PledgeApplicationId &&
                    c.IsOnFlexiPaymentPilot == Request.IsOnFlexiPaymentPilot &&
                    c.IsProviderOnFlexiPaymentPilot == Request.IsProviderOnFlexiPaymentPilot &&
                    c.UserInfo == Request.UserInfo), CancellationToken.None))
                .ReturnsAsync(Result);
        }

        public Task<IActionResult> Create()
        {
            return Controller.Create(Request);
        }
    }
}
